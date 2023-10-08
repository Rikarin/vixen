namespace Rin.Core.UI;

public class State<T> {
    event Action<T> callbacks;
    
    public T Value { get; private set; }

    public void Subscribe(Action<T> callback) {
        callbacks += callback;
    }

    public void SetNext(T value) {
        Value = value;
        callbacks?.Invoke(value);
    }
}
