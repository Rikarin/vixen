namespace Rin.Core.MicroThreading;

struct MicroThreadCallbackList {
    public MicroThreadCallbackNode First { get; private set; }
    public MicroThreadCallbackNode Last { get; private set; }

    public void Add(MicroThreadCallbackNode node) {
        if (First == null) {
            First = node;
        } else {
            Last.Next = node;
        }

        Last = node;
    }

    public bool TakeFirst(out MicroThreadCallbackNode callback) {
        callback = First;

        if (First == null) {
            return false;
        }

        First = callback.Next;
        callback.Next = null;

        return true;
    }
}
