namespace Rin.Core.UI;

public class TableColumn<T> : View {
    public string Header { get; }
    public Func<T, string>? Formatter { get; }
    public Func<T, View>? Content { get; }


    public TableColumn(string header, Func<T, string> formatter) {
        Header = header;
        Formatter = formatter;
    }

    public TableColumn(string header, Func<T, View> content) {
        Header = header;
        Content = content;
    }
}