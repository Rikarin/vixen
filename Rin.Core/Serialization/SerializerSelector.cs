using Rin.Core.Storage;
using System.Text;

namespace Rin.Core.Serialization;

public delegate void SerializeObjectDelegate(SerializationStream stream, ref object obj, ArchiveMode archiveMode);

/// <summary>
///     Serializer context. It holds DataSerializer{T} objects and their factories.
/// </summary>
public class SerializerSelector {
    public SerializerSelector SelectorOverride;
    
    readonly object @lock = new();
    readonly string[] profiles;
    Dictionary<Type, DataSerializer> dataSerializersByType = new();
    Dictionary<ObjectId, DataSerializer> dataSerializersByTypeId = new();

    bool invalidated;
    int dataSerializerFactoryVersion;

    /// <summary>
    ///     Gets the default instance of Serializer.
    /// </summary>
    /// <value>
    ///     The default instance.
    /// </value>
    public static SerializerSelector Default { get; internal set; }

    public static SerializerSelector Asset { get; internal set; }
    public static SerializerSelector AssetWithReuse { get; internal set; }
    public IEnumerable<string> Profiles => profiles;

    /// <summary>
    ///     Gets whether serialization reuses references, where each reference gets assigned an ID and if it is serialized
    ///     again, same instance will be reused).
    /// </summary>
    public bool ReuseReferences { get; }

    /// <summary>
    ///     Gets whether <see cref="IIdentifiable" /> instances marked as external will have only their <see cref="Guid" />
    ///     stored.
    /// </summary>
    public bool ExternalIdentifiableAsGuid { get; }

    public List<SerializerFactory> SerializerFactories { get; } = new();

    static SerializerSelector() {
        // Do a two step initialization to make sure field is set and accessible during construction
        Default = new(false, -1, "Default");
        Default.Initialize();

        Asset = new(false, -1, "Default", "Content");
        Asset.Initialize();

        AssetWithReuse = new(true, -1, "Default", "Content");
        AssetWithReuse.Initialize();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializerSelector" /> class.
    /// </summary>
    /// <param name="reuseReferences">if set to <c>true</c> reuse references (allow cycles in the object graph).</param>
    /// <param name="profiles">The profiles.</param>
    public SerializerSelector(bool reuseReferences, bool externalIdentifiableAsGuid, params string[] profiles) {
        ReuseReferences = reuseReferences;
        ExternalIdentifiableAsGuid = externalIdentifiableAsGuid;
        
        if (externalIdentifiableAsGuid && !reuseReferences) {
            throw new NotImplementedException(
                "Support of ExternalIdentifiableAsGuid without ReuseReferences is not implemented yet."
            );
        }

        this.profiles = profiles;
        Initialize();
    }

    public SerializerSelector(params string[] profiles) : this(false, false, profiles) { }

    SerializerSelector(bool reuseReferences, int unusedPrivateCtor, params string[] profiles) {
        ReuseReferences = reuseReferences;
        this.profiles = profiles;
    }

    /// <summary>
    ///     Checks if this instance supports the specified serialization profile.
    /// </summary>
    /// <param name="profile">Name of the profile</param>
    /// <returns><c>true</c> if this instance supports the specified serialization profile</returns>
    public bool HasProfile(string profile) {
        if (profile == null) {
            throw new ArgumentNullException(nameof(profile));
        }

        return profiles.Any(p => profile == p);
    }

    public DataSerializer? GetSerializer(ref ObjectId typeId) {
        if (invalidated) {
            UpdateDataSerializers();
        }

        if (!dataSerializersByTypeId.TryGetValue(typeId, out var dataSerializer)) {
            foreach (var serializerFactory in SerializerFactories) {
                dataSerializer = serializerFactory.GetSerializer(this, ref typeId);
                if (dataSerializer != null) {
                    break;
                }
            }
        }

        if (dataSerializer is { Initialized: false }) {
            EnsureInitialized(dataSerializer);
        }

        return dataSerializer;
    }

    /// <summary>
    ///     Gets the serializer.
    /// </summary>
    /// <param name="type">The type that you want to (de)serialize.</param>
    /// <returns>The <see cref="DataSerializer{T}" /> for this type if it exists or can be created, otherwise null.</returns>
    public DataSerializer? GetSerializer(Type type) {
        if (invalidated) {
            UpdateDataSerializers();
        }

        if (!dataSerializersByType.TryGetValue(type, out var dataSerializer)) {
            foreach (var serializerFactory in SerializerFactories) {
                dataSerializer = serializerFactory.GetSerializer(this, type);
                if (dataSerializer != null) {
                    break;
                }
            }
        }

        if (dataSerializer != null && !dataSerializer.Initialized) {
            EnsureInitialized(dataSerializer);
        }

        return dataSerializer;
    }

    /// <summary>
    ///     Internal function, for use by <see cref="SerializerFactory" />.
    /// </summary>
    /// <param name="dataSerializer">The data serializer to initialize if not done yet</param>
    public void EnsureInitialized(DataSerializer dataSerializer) {
        // Allow reentry (in case a serializer needs itself)
        if (dataSerializer.InitializeLock.IsHeldByCurrentThread) {
            return;
        }

        var gotLock = false;
        try {
            dataSerializer.InitializeLock.Enter(ref gotLock);

            // Ensure a serialization type ID has been generated (otherwise do so now)
            EnsureSerializationTypeId(dataSerializer);

            if (!dataSerializer.Initialized) {
                // Initialize (if necessary)
                dataSerializer.Initialize(SelectorOverride ?? this);

                // Mark it as initialized
                dataSerializer.Initialized = true;
            }
        } finally {
            if (gotLock) {
                dataSerializer.InitializeLock.Exit();
            }
        }
    }

    /// <summary>
    ///     Gets the serializer.
    /// </summary>
    /// <typeparam name="T">The type that you want to (de)serialize.</typeparam>
    /// <returns>The <see cref="DataSerializer{T}" /> for this type if it exists or can be created, otherwise null.</returns>
    public DataSerializer<T>? GetSerializer<T>() => (DataSerializer<T>)GetSerializer(typeof(T));

    void Initialize() {
        invalidated = true;
        DataSerializerFactory.RegisterSerializerSelector(this);
        UpdateDataSerializers();
    }

    static void EnsureSerializationTypeId(DataSerializer dataSerializer) {
        // Ensure a serialization type ID has been generated (otherwise do so now)
        if (dataSerializer.SerializationTypeId == ObjectId.Empty) {
            // Need to generate serialization type id
            var typeName = dataSerializer.SerializationType.FullName;
            dataSerializer.SerializationTypeId = ObjectId.FromBytes(Encoding.UTF8.GetBytes(typeName));
        }
    }

    void UpdateDataSerializers() {
        if (invalidated) {
            var newDataSerializersByType = new Dictionary<Type, DataSerializer>();
            var newDataSerializersByTypeId = new Dictionary<ObjectId, DataSerializer>();

            // Create list of combined serializers
            var combinedSerializers = new Dictionary<Type, AssemblySerializerEntry>();

            int capturedVersion;

            lock (DataSerializerFactory.Lock) {
                foreach (var profile in profiles) {
                    if (DataSerializerFactory.DataSerializersPerProfile.TryGetValue(
                            profile,
                            out Dictionary<Type, AssemblySerializerEntry> serializersPerProfile
                        )) {
                        foreach (var serializer in serializersPerProfile) {
                            combinedSerializers[serializer.Key] = serializer.Value;
                        }
                    }
                }

                // Due to multithreading, maybe the current version is already that one, or better
                // In this case, we can stop right there
                capturedVersion = DataSerializerFactory.Version;
                if (dataSerializerFactoryVersion >= capturedVersion) {
                    invalidated = false;
                    return;
                }
            }

            // Create new list of serializers (it will create new ones, and remove unused ones)
            foreach (var serializer in combinedSerializers) {
                if (!dataSerializersByType.TryGetValue(serializer.Key, out var dataSerializer)) {
                    if (serializer.Value.SerializerType != null) {
                        // New serializer, let's create it
                        dataSerializer = (DataSerializer)Activator.CreateInstance(serializer.Value.SerializerType);
                        dataSerializer.SerializationTypeId = serializer.Value.Id;

                        // Ensure a serialization type ID has been generated (otherwise do so now)
                        EnsureSerializationTypeId(dataSerializer);
                    }
                }

                newDataSerializersByType[serializer.Key] = dataSerializer;
                newDataSerializersByTypeId[serializer.Value.Id] = dataSerializer;
            }

            // Do the actual state switch inside a lock
            lock (@lock) {
                // Due to multithreading, make sure we really still need to update
                if (dataSerializerFactoryVersion < capturedVersion) {
                    dataSerializerFactoryVersion = capturedVersion;
                    dataSerializersByType = newDataSerializersByType;
                    dataSerializersByTypeId = newDataSerializersByTypeId;
                }

                invalidated = false;
            }
        }
    }

    internal void Invalidate() {
        invalidated = true;
    }
}
