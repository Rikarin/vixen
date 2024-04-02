using System.Collections;
using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Represents an YAML stream.
/// </summary>
public class YamlStream : IEnumerable<YamlDocument> {
    /// <summary>
    ///     Gets the documents inside the stream.
    /// </summary>
    /// <value>The documents.</value>
    public IList<YamlDocument> Documents { get; } = new List<YamlDocument>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlStream" /> class.
    /// </summary>
    public YamlStream() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlStream" /> class.
    /// </summary>
    public YamlStream(params YamlDocument[] documents) : this((IEnumerable<YamlDocument>)documents) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlStream" /> class.
    /// </summary>
    public YamlStream(IEnumerable<YamlDocument> documents) {
        foreach (var document in documents) {
            Documents.Add(document);
        }
    }

    /// <summary>
    ///     Adds the specified document to the <see cref="Documents" /> collection.
    /// </summary>
    /// <param name="document">The document.</param>
    public void Add(YamlDocument document) {
        Documents.Add(document);
    }

    /// <summary>
    ///     Loads the stream from the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    public void Load(TextReader input) {
        Documents.Clear();

        var parser = new Parser(input);
        var events = new EventReader(parser);
        events.Expect<StreamStart>();
        while (!events.Accept<StreamEnd>()) {
            var document = new YamlDocument(events);
            Documents.Add(document);
        }

        events.Expect<StreamEnd>();
    }

    /// <summary>
    ///     Saves the stream to the specified output.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="isLastDocumentEndImplicit">If set to <c>true</c>, last <see cref="DocumentEnd" /> will be implicit.</param>
    /// <param name="bestIndent">The desired indent.</param>
    public void Save(
        TextWriter output,
        bool isLastDocumentEndImplicit = false,
        int bestIndent = Emitter.MinBestIndent
    ) {
        var emitter = new Emitter(output, bestIndent);
        emitter.Emit(new StreamStart());

        var lastDocument = Documents.Count > 0 ? Documents[^1] : null;
        foreach (var document in Documents) {
            var isDocumentEndImplicit = isLastDocumentEndImplicit && document == lastDocument;
            document.Save(emitter, isDocumentEndImplicit);
        }

        emitter.Emit(new StreamEnd());
    }

    /// <summary>
    ///     Accepts the specified visitor by calling the appropriate Visit method on it.
    /// </summary>
    /// <param name="visitor">
    ///     A <see cref="IYamlVisitor" />.
    /// </param>
    public void Accept(IYamlVisitor visitor) {
        visitor.Visit(this);
    }

    #region IEnumerable<YamlDocument> Members

    /// <summary />
    public IEnumerator<YamlDocument> GetEnumerator() => Documents.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
