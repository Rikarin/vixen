using Rin.Editor.RinCompiler.Ast;

namespace Rin.Editor.RinCompiler.Parser;

public partial class Parser {
    TokenRange range;

    public ShaderDeclaration Parse(TokenRange tokenRange) {
        range = tokenRange;
        range.Match(TokenType.Begin);

        return ParseShaderDeclaration();
    }

    ShaderDeclaration ParseShaderDeclaration() {
        range.Match(TokenType.Shader);
        var loc = range.Front.Location;

        var name = range.Front.Name;
        range.Match(TokenType.StringLiteral);

        var declarations = ParseAggregate();
        return new(loc.SpanTo(range.Previous), name, declarations);
    }

    List<Declaration> ParseAggregate() {
        var declarations = new List<Declaration>();
        range.Match(TokenType.OpenBrace);
        while (range.Front.Type != TokenType.CloseBrace && !range.IsEmpty) {
            declarations.Add(ParseDeclaration());
        }

        range.Match(TokenType.CloseBrace);
        return declarations;
    }

    Declaration ParseDeclaration() {
        switch (range.Front.Type) {
            case TokenType.Properties:
                return ParseProperties();
            case TokenType.SubShader:
                return ParseSubShader();

            default: throw new($"Unknown identifier {range.Front.Name.Value}");
        }
    }

    void ParseSubShaderDeclaration(
        ref Dictionary<string, string> tags,
        ref int? lod,
        ref SubShaderPassDeclaration? program
    ) {
        switch (range.Front.Type) {
            case TokenType.Tags:
                range.PopFront();
                range.Match(TokenType.OpenBrace);

                while (!range.IsEmpty) {
                    var key = range.Front.Name;
                    range.Match(TokenType.StringLiteral);
                    range.Match(TokenType.Equal);
                    var value = range.Front.Name;
                    range.Match(TokenType.StringLiteral);

                    tags[key.Value] = value.Value;

                    if (range.Front.Type == TokenType.Comma) {
                        range.PopFront();
                    } else {
                        range.Match(TokenType.CloseBrace);
                        break;
                    }
                }

                break;

            case TokenType.Lod:
                range.PopFront();
                lod = Convert.ToInt32(range.Front.Name.Value);
                range.Match(TokenType.IntegerLiteral);
                break;

            case TokenType.Pass:
                program = ParseSubShaderPass();
                break;

            default: throw new($"Unknown identifier {range.Front.Name.Value}");
        }
    }

    SubShaderPassDeclaration ParseSubShaderPass() {
        var loc = range.Front.Location;
        range.PopFront();
        range.Match(TokenType.OpenBrace);

        var program = range.Front.Name;
        range.Match(TokenType.HlslProgram);
        range.Match(TokenType.CloseBrace);

        return new(loc.SpanTo(range.Previous), program);
    }

    Declaration ParseSubShader() {
        var loc = range.Front.Location;
        range.PopFront();
        range.Match(TokenType.OpenBrace);

        var tags = new Dictionary<string, string>();
        int? lod = null;
        SubShaderPassDeclaration? shaderProgram = null;

        while (range.Front.Type != TokenType.CloseBrace && !range.IsEmpty) {
            ParseSubShaderDeclaration(ref tags, ref lod, ref shaderProgram);
        }

        range.Match(TokenType.CloseBrace);
        return new SubShaderDeclaration(loc.SpanTo(range.Previous), tags, lod, shaderProgram);
    }

    PropertiesDeclaration ParseProperties() {
        var loc = range.Front.Location;
        range.Match(TokenType.Properties);
        range.Match(TokenType.OpenBrace);

        var declarations = new List<MaterialPropertyDeclaration>();
        while (range.Front.Type != TokenType.CloseBrace && !range.IsEmpty) {
            declarations.Add(ParseMaterialPropertyDeclaration());
        }

        range.Match(TokenType.CloseBrace);
        return new(loc.SpanTo(range.Previous), declarations);
    }

    MaterialPropertyDeclaration ParseMaterialPropertyDeclaration() {
        // _MainTex ("Texture", 2D) = "white"
        var loc = range.Front.Location;
        var identifier = range.Front.Name;
        range.Match(TokenType.Identifier);

        range.Match(TokenType.OpenParen);
        var name = range.Front.Name;
        range.Match(TokenType.StringLiteral);
        range.Match(TokenType.Comma);
        var typeDecl = ParseTypeDeclaration();
        range.Match(TokenType.CloseParen);

        // TODO: are default values mandatory or optional?
        range.Match(TokenType.Equal);
        var value = ParsePrimaryExpression();

        return new(loc.SpanTo(range.Previous), identifier, name, typeDecl, value);
    }

    TypeDeclaration ParseTypeDeclaration() {
        var loc = range.Front.Location;
        var baseTypes = new[] {
            TokenType.Tex2D, TokenType.Tex3D, TokenType.Tex2DArray, TokenType.Int, TokenType.Float, TokenType.Cube,
            TokenType.CubeArray, TokenType.Color, TokenType.Vector
        };

        // Try to match basic types
        var token = range.Front;
        if (baseTypes.Contains(token.Type)) {
            range.PopFront();
            return new(loc, token.Type);
        }

        // Match Range
        range.Match(TokenType.Range);
        range.Match(TokenType.OpenParen);
        var from = Convert.ToSingle(range.Front.Name.Value);
        range.Match(TokenType.IntegerLiteral);
        range.Match(TokenType.Comma);
        var to = Convert.ToSingle(range.Front.Name.Value);
        range.Match(TokenType.IntegerLiteral);
        range.Match(TokenType.CloseParen);

        return new RangeTypeDeclaration(loc.SpanTo(range.Previous), from, to);
    }
}
