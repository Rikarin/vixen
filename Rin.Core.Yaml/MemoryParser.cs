using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml;

public class MemoryParser : IParser {
    int position = -1;

    public IList<ParsingEvent> ParsingEvents { get; }

    /// <inheritdoc />
    public ParsingEvent? Current { get; private set; }

    /// <inheritdoc />
    public bool IsEndOfStream => position >= ParsingEvents.Count;

    public int Position {
        get => position;
        set {
            position = value;
            Current = position >= 0 ? ParsingEvents[position] : null;
        }
    }

    public MemoryParser(IList<ParsingEvent> parsingEvents) {
        ParsingEvents = parsingEvents;
    }

    public bool MoveNext() {
        if (++position < ParsingEvents.Count) {
            Current = ParsingEvents[position];
            return true;
        }

        return false;
    }
}
