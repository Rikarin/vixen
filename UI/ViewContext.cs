namespace Rin.UI;

public static class ViewContext {
    static int counter;

    public static int GetId() => counter++;
    public static void Reset() => counter = 0;
}
