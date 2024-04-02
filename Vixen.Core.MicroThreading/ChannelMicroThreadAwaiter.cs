using System.Runtime.CompilerServices;

namespace Vixen.Core.MicroThreading;

public class ChannelMicroThreadAwaiter<T> : ICriticalNotifyCompletion {
    internal MicroThread? MicroThread;
    internal Action Continuation;
    internal T Result;
    static readonly List<ChannelMicroThreadAwaiter<T>> pool = new();

    bool isCompleted;

    public bool IsCompleted {
        get => isCompleted || MicroThread is { IsOver: true };
        set => isCompleted = value;
    }

    public ChannelMicroThreadAwaiter(MicroThread microThread) {
        MicroThread = microThread;
    }

    public static ChannelMicroThreadAwaiter<T> New(MicroThread microThread) {
        lock (pool) {
            if (pool.Count > 0) {
                var index = pool.Count - 1;
                var lastItem = pool[index];
                pool.RemoveAt(index);

                lastItem.MicroThread = microThread;
                return lastItem;
            }

            return new(microThread);
        }
    }

    public ChannelMicroThreadAwaiter<T> GetAwaiter() => this;

    public void OnCompleted(Action continuation) {
        Continuation = continuation;
    }

    public void UnsafeOnCompleted(Action continuation) {
        Continuation = continuation;
    }

    public T GetResult() {
        // Check Task Result (exception, etc...)
        MicroThread.CancellationToken.ThrowIfCancellationRequested();

        var result = Result;

        // After result has been taken, we can reuse this item, so put it in the pool
        // We mitigate pool size, but another approach than hard limit might be interesting
        lock (pool) {
            if (pool.Count < 4096) {
                isCompleted = false;
                MicroThread = null;
                Continuation = null;
                Result = default;
            }

            pool.Add(this);
        }

        return result;
    }
}
