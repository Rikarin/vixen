using Rin.Editor.RinCompiler.Ast;

namespace Rin.Editor.RinCompiler.Parser;

public partial class Parser {
    Expression ParsePrimaryExpression() {
        var loc = range.Front.Location;

        switch (range.Front.Type) {
            case TokenType.IntegerLiteral:
                return ParseIntegerLiteral();
            case TokenType.CharacterLiteral:
                return ParseCharacterLiteral();
            case TokenType.StringLiteral:
                return ParseStringLiteral();

            // TODO: vectors, colors....

            default: throw new NotImplementedException();
        }
    }

    IntegerLiteral ParseIntegerLiteral() {
        var loc = range.Front.Location;
        var strValue = Convert.ToUInt64(range.Front.Name.Value);

        range.PopFront();
        // TODO: expand types
        return new(loc, strValue, BuiltinType.Int);
    }

    CharacterLiteral ParseCharacterLiteral() {
        var loc = range.Front.Location;
        var value = range.Front.Name.Value[0];

        range.PopFront();
        // TODO: expand types
        return new(loc, value, BuiltinType.Char);
    }

    StringLiteral ParseStringLiteral() {
        var loc = range.Front.Location;
        var strValue = range.Front.Name.Value;

        range.PopFront();
        return new(loc, strValue, BuiltinType.String);
    }
}
