using Antlr4.Runtime;

namespace Rin.Core.Shaders;

public abstract class RinParserBase : Parser {
    public RinParserBase(ITokenStream input) : base(input) { }
}
