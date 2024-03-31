using Rin.Core.Yaml.Events;
using System.Globalization;
using Event = Rin.Core.Yaml.Events.ParsingEvent;

namespace Rin.Core.Yaml;

/// <summary>
///     Reads events from a sequence of <see cref="Event" />.
/// </summary>
public class EventReader {
    bool endOfStream;

    /// <summary>
    ///     Gets the underlying parser.
    /// </summary>
    /// <value>The parser.</value>
    public IParser Parser { get; }

    public int CurrentDepth { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventReader" /> class.
    /// </summary>
    /// <param name="parser">The parser that provides the events.</param>
    public EventReader(IParser parser) {
        Parser = parser;
        MoveNext();
    }

    /// <summary>
    ///     Ensures that the current event is of the specified type, returns it and moves to the next event.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Event" />.</typeparam>
    /// <returns>Returns the current event.</returns>
    /// <exception cref="YamlException">If the current event is not of the specified type.</exception>
    public T Expect<T>() where T : Event {
        var yamlEvent = Allow<T>();
        if (yamlEvent == null) {
            // TODO: Throw a better exception
            throw new YamlException(
                Parser.Current.Start,
                Parser.Current.End,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Expected '{0}', got '{1}' (at line {2}, character {3}).",
                    typeof(T).Name,
                    Parser.Current.GetType().Name,
                    Parser.Current.Start.Line,
                    Parser.Current.Start.Column
                )
            );
        }

        return yamlEvent;
    }

    /// <summary>
    ///     Checks whether the current event is of the specified type.
    /// </summary>
    /// <typeparam name="T">Type of the event.</typeparam>
    /// <returns>Returns true if the current event is of type <typeparamref name="T" />. Otherwise returns false.</returns>
    public bool Accept<T>() where T : Event {
        EnsureNotAtEndOfStream();
        return Parser.Current is T;
    }

    /// <summary>
    ///     Checks whether the current event is of the specified type.
    ///     If the event is of the specified type, returns it and moves to the next event.
    ///     Otherwise returns null.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Event" />.</typeparam>
    /// <returns>Returns the current event if it is of type T; otherwise returns null.</returns>
    public T? Allow<T>() where T : Event {
        if (!Accept<T>()) {
            return null;
        }

        var yamlEvent = (T)Parser.Current;
        MoveNext();
        return yamlEvent;
    }

    /// <summary>
    ///     Gets the next event without consuming it.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Event" />.</typeparam>
    /// <returns>Returns the current event if it is of type T; otherwise returns null.</returns>
    public T? Peek<T>() where T : Event {
        if (!Accept<T>()) {
            return null;
        }

        var yamlEvent = (T)Parser.Current;
        return yamlEvent;
    }

    public void ReadCurrent(IList<Event> events) {
        var depth = 0;

        do {
            if (Accept<SequenceStart>() || Accept<MappingStart>() || Accept<StreamStart>() || Accept<DocumentStart>()) {
                ++depth;
            } else if (Accept<SequenceEnd>() || Accept<MappingEnd>() || Accept<StreamEnd>() || Accept<DocumentEnd>()) {
                --depth;
            }

            events.Add(Allow<Event>());
        } while (depth > 0 && !endOfStream);
    }

    /// <summary>
    ///     Skips the current event and any "child" event.
    /// </summary>
    public void Skip() {
        var depth = 0;

        do {
            if (Accept<SequenceStart>() || Accept<MappingStart>() || Accept<StreamStart>() || Accept<DocumentStart>()) {
                ++depth;
            } else if (Accept<SequenceEnd>() || Accept<MappingEnd>() || Accept<StreamEnd>() || Accept<DocumentEnd>()) {
                --depth;
            }

            MoveNext();
        } while (depth > 0 && !endOfStream);
    }

    /// <summary>
    ///     Skips until we reach the appropriate depth again
    /// </summary>
    public void Skip(int untilDepth, bool skipAtLeastOne = true) {
        while (CurrentDepth > untilDepth || skipAtLeastOne) {
            MoveNext();
            skipAtLeastOne = false;
        }
    }

    /// <summary>
    ///     Call this if <see cref="Parser" /> state has changed (i.e. it might not be at end of stream anymore).
    /// </summary>
    public void RefreshParserState() {
        endOfStream = Parser.IsEndOfStream;
    }

    /// <summary>
    ///     Moves to the next event.
    /// </summary>
    void MoveNext() {
        if (Parser.Current != null) {
            CurrentDepth += Parser.Current.NestingIncrease;
        }

        endOfStream = !Parser.MoveNext();
    }

    /// <summary>
    ///     Throws an exception if Ensures the not at end of stream.
    /// </summary>
    void EnsureNotAtEndOfStream() {
        if (endOfStream) {
            throw new EndOfStreamException();
        }
    }
}
