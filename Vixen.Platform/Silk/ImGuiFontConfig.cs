namespace Vixen.Platform.Silk; 

public readonly struct ImGuiFontConfig {
    public string FontPath { get; }
    public int FontSize { get; }

    public ImGuiFontConfig(string fontPath, int fontSize) {
        if (fontSize <= 0) {
            throw new ArgumentOutOfRangeException(nameof(fontSize));
        }

        FontPath = fontPath ?? throw new ArgumentNullException(nameof(fontPath));
        FontSize = fontSize;
    }
}
