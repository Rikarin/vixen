namespace Rin.UI;

public class State<T> {
    event Action<T> callbacks;
    
    public T Value { get; protected set; }

    public void Subscribe(Action<T> callback) {
        callbacks += callback;
    }

    public virtual void SetNext(T value, bool sendEvent = true) {
        Value = value;
        
        if (sendEvent) {
            callbacks?.Invoke(value);
        }
    }
}
