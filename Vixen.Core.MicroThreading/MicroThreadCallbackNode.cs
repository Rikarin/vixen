namespace Vixen.Core.MicroThreading;

class MicroThreadCallbackNode {
    public Action? MicroThreadAction;
    public SendOrPostCallback? SendOrPostCallback;
    public object? CallbackState;
    public MicroThreadCallbackNode? Next;

    public void Invoke() {
        if (MicroThreadAction != null) {
            MicroThreadAction();
        } else {
            SendOrPostCallback(CallbackState);
        }
    }

    public void Clear() {
        MicroThreadAction = null;
        SendOrPostCallback = null;
        CallbackState = null;
    }
}
