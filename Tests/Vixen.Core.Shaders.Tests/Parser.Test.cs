using Antlr4.Runtime;
using Vixen.Core.Shaders.Ast;
using Xunit;

namespace Vixen.Core.Shaders.Tests; 

public class Parser_Test {
    [Fact]
    void TestParser() {
        // var visitor = new rules
        var stream = new AntlrInputStream(File.ReadAllText("../../../Example1.rsh"));
        var lexer = new VixenLexer(stream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new VixenParser(tokenStream);

        var visitor = new BuildAstVisitor();
        
        // Entrypoint?
        var tree = parser.compilation_unit();
        var module = tree.Accept(visitor) as Module;
        
        Assert.Equal("Vixen.Test", module.Package.Name.Text);

        var shader = module.Declarations.OfType<Shader>().First();
        Assert.Equal("TestShader", shader.Name.Text);
        
        Assert.Equal(2, shader.Declarations.OfType<ConstructorDeclaration>().Count());

        var testMethod = shader.Declarations.OfType<MethodDeclaration>().First(x => x.Name == "TestMethod");
        Assert.Equal("name", testMethod.Parameters[0].Name);
        Assert.Equal("count", testMethod.Parameters[1].Name);
        
        Assert.Equal(12, shader.Declarations.Count);
    }
}
