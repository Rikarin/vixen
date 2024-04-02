using Antlr4.Runtime;

namespace Vixen.Core.Shaders;

public abstract class VixenParserBase(ITokenStream input) : Parser(input);
