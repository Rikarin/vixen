using Serilog;
using Silk.NET.SPIRV;
using Silk.NET.SPIRV.Cross;
using System.Runtime.InteropServices;

namespace Rin.Editor;

sealed unsafe class CrossWrapper : IDisposable {
    readonly Cross cross = Cross.GetApi();
    Context* context;

    public CrossWrapper() {
        fixed (Context** ptr = &context) {
            cross.ContextCreate(ptr);
            cross.ContextSetErrorCallback(context, PfnErrorCallback.From(ErrorHandler), null);
        }
    }

    public CompilerWrapper CreateCompiler(ReadOnlySpan<byte> irCode) => new(this, irCode);


    public void Dispose() {
        cross.ContextDestroy(context);
    }

    void ErrorHandler(void* arg0, byte* arg1) {
        // TODO call some OnError event
        var str = Marshal.PtrToStringAnsi((nint)arg1);
        Log.Information("Error Callback Spir-V Cross {str}", str);
    }

    public sealed class CompilerWrapper {
        internal readonly CrossWrapper cross;
        internal Compiler* compiler;

        public CompilerWrapper(CrossWrapper cross, ReadOnlySpan<byte> irCode) {
            this.cross = cross;

            fixed (Compiler** compilerPtr = &compiler)
            fixed (byte* irCodePtr = irCode) {
                ParsedIr* parsedIr;

                cross.cross.ContextParseSpirv(
                    cross.context,
                    (uint*)irCodePtr,
                    (nuint)irCode.Length / sizeof(uint),
                    &parsedIr
                );

                cross.cross.ContextCreateCompiler(
                    cross.context,
                    Backend.Glsl,
                    parsedIr,
                    CaptureMode.TakeOwnership,
                    compilerPtr
                );
            }
        }

        public ResourcesWrapper CreateShaderResources() => new(this);
    }

    public sealed class ResourcesWrapper {
        readonly Cross cross;
        readonly CompilerWrapper compiler;
        Resources* resources;

        public ResourcesWrapper(CompilerWrapper compiler) {
            this.compiler = compiler;
            cross = compiler.cross.cross;

            fixed (Resources** resourcesPtr = &resources) {
                cross.CompilerCreateShaderResources(compiler.compiler, resourcesPtr);
            }
        }

        public ResourceWrapper[] GetResourceListForType(ResourceType type) {
            ReflectedResource* list;
            UIntPtr count;

            cross.ResourcesGetResourceListForType(resources, type, &list, &count);
            return new Span<ReflectedResource>(list, (int)count)
                .ToArray()
                .Select(x => new ResourceWrapper(compiler, x))
                .ToArray();
        }
    }

    public sealed class ResourceWrapper {
        readonly Cross cross;
        readonly CompilerWrapper compiler;
        CrossType* bufferType;
        int? descriptorSet;
        int? binding;
        int? declaredStructSize;
        int? memberCount;
        bool? isActive;

        public uint Id { get; }
        public uint BaseTypeId { get; }
        public uint TypeId { get; }
        public string Name { get; }

        public int DescriptorSet =>
            descriptorSet ??= (int)cross.CompilerGetDecoration(compiler.compiler, Id, Decoration.DescriptorSet);

        public int BindingPoint =>
            binding ??= (int)cross.CompilerGetDecoration(compiler.compiler, Id, Decoration.Binding);

        public int MemberCount => memberCount ??= (int)cross.TypeGetNumMemberTypes(BufferType);

        public int DeclaredStructSize {
            get {
                if (declaredStructSize == null) {
                    UIntPtr size = 0;
                    cross.CompilerGetDeclaredStructSize(compiler.compiler, BufferType, &size);
                    declaredStructSize = (int)size;
                }

                return declaredStructSize.Value;
            }
        }

        public bool IsActive {
            get {
                if (isActive == null) {
                    UIntPtr activeNum = 0;
                    BufferRange* range;
                    cross.CompilerGetActiveBufferRanges(compiler.compiler, Id, &range, &activeNum);

                    isActive = activeNum != 0;
                }

                return isActive.Value;
            }
        }

        CrossType* BufferType {
            get {
                if (bufferType == null) {
                    bufferType = cross.CompilerGetTypeHandle(compiler.compiler, BaseTypeId);
                }

                return bufferType;
            }
        }

        public (int Type, string Name, int Size, int Offset) GetMemoryInfo(int index) {
            // TODO: not sure which one to use
            // var lol = cross.TypeGetBasetype(bufferType);
            var typeId = cross.TypeGetBaseTypeId(bufferType);
            var type = cross.TypeGetMemberType(bufferType, (uint)index);
            var memberName = cross.CompilerGetMemberNameS(compiler.compiler, typeId, (uint)index);
            
            UIntPtr size = 0;
            cross.CompilerGetDeclaredStructMemberSize(compiler.compiler, bufferType, (uint)index, &size);

            uint offset = 0;
            cross.CompilerTypeStructMemberOffset(compiler.compiler, BufferType, (uint)index, &offset);
            return ((int)type, memberName, (int)size, (int)offset);
        }

        public ResourceWrapper(CompilerWrapper compiler, ReflectedResource resource) {
            cross = compiler.cross.cross;
            this.compiler = compiler;

            Id = resource.Id;
            BaseTypeId = resource.BaseTypeId;
            TypeId = resource.TypeId;
            Name = Marshal.PtrToStringAnsi((nint)resource.Name)!;
        }
    }
}
