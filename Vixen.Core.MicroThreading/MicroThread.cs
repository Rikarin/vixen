using System.Diagnostics;
using Vixen.Core.Collections;
using Vixen.Diagnostics;

namespace Vixen.Core.MicroThreading;

/// <summary>
///     Represents an execution context managed by a <see cref="Scheduler" />, that can cooperatively yield execution to
///     another <see cref="MicroThread" /> at any point (usually using async calls).
/// </summary>
public class MicroThread {
    /// <summary>
    ///     Gets the attached properties to this component.
    /// </summary>
    public PropertyContainer Tags;

    internal ProfilingKey? ProfilingKey;
    internal PriorityQueueNode<SchedulerEntry> ScheduledLinkedListNode;
    internal LinkedListNode<MicroThread> AllLinkedListNode; // Also used as lock for "CompletionTask"
    internal MicroThreadCallbackList Callbacks;
    internal SynchronizationContext SynchronizationContext;

    static long globalCounterId;

    int state;
    readonly CancellationTokenSource cancellationTokenSource;

    /// <summary>
    ///     Gets or sets the priority of this <see cref="MicroThread" />.
    /// </summary>
    /// <value>
    ///     The priority.
    /// </value>
    public long Priority {
        get => ScheduledLinkedListNode.Value.Priority;
        set {
            if (ScheduledLinkedListNode.Value.Priority != value) {
                Reschedule(ScheduleMode.First, value);
            }
        }
    }

    /// <summary>
    ///     Gets the id of this <see cref="MicroThread" />.
    /// </summary>
    /// <value>
    ///     The id.
    /// </value>
    public long Id { get; private set; }

    /// <summary>
    ///     Gets or sets the name of this <see cref="MicroThread" />.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    ///     Gets the scheduler associated with this <see cref="MicroThread" />.
    /// </summary>
    /// <value>The scheduler associated with this <see cref="MicroThread" />.</value>
    public Scheduler Scheduler { get; private set; }

    /// <summary>
    ///     Gets the state of this <see cref="MicroThread" />.
    /// </summary>
    /// <value>The state of this <see cref="MicroThread" />.</value>
    public MicroThreadState State {
        get => (MicroThreadState)state;
        internal set => state = (int)value;
    }

    /// <summary>
    ///     Gets the exception that was thrown by this <see cref="MicroThread" />.
    /// </summary>
    /// It could come from either internally, or from
    /// <see cref="RaiseException" />
    /// if it was successfully processed.
    /// <value>The exception.</value>
    public Exception? Exception { get; private set; }

    /// <summary>
    ///     Gets the <see cref="MicroThread" /> flags.
    /// </summary>
    /// <value>
    ///     The flags.
    /// </value>
    public MicroThreadFlags Flags { get; private set; }

    /// <summary>
    ///     Gets or sets the <see cref="MicroThread" /> scheduling mode.
    /// </summary>
    /// <value>
    ///     The scheduling mode.
    /// </value>
    public ScheduleMode ScheduleMode { get; set; }

    /// <summary>
    ///     A token for listening to the cancellation of the MicroThread.
    /// </summary>
    public CancellationToken CancellationToken => cancellationTokenSource.Token;

    /// <summary>
    ///     Indicates whether the MicroThread is terminated or not, either in Completed, Canceled or Failed status.
    /// </summary>
    public bool IsOver => State is MicroThreadState.Completed or MicroThreadState.Canceled or MicroThreadState.Failed;

    /// <summary>
    ///     Gets the current micro thread (self).
    /// </summary>
    /// <value>The current micro thread (self).</value>
    public static MicroThread Current => Scheduler.CurrentMicroThread;

    /// <summary>
    ///     Gets or sets the task that will be executed upon completion (used internally for <see cref="Scheduler.WhenAll" />)
    /// </summary>
    internal TaskCompletionSource<int>? CompletionTask { get; set; }

    public MicroThread(Scheduler scheduler, MicroThreadFlags flags = MicroThreadFlags.None) {
        Id = Interlocked.Increment(ref globalCounterId);
        Scheduler = scheduler;
        ScheduledLinkedListNode = new(new(this));
        AllLinkedListNode = new(this);
        ScheduleMode = ScheduleMode.Last;
        Flags = flags;
        Tags = new(this);
        cancellationTokenSource = new();
    }

    public void Migrate(Scheduler scheduler) {
        throw new NotImplementedException();
    }

    public void Remove() {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Starts this <see cref="MicroThread" /> with the specified function.
    /// </summary>
    /// <param name="microThreadFunction">The micro thread function.</param>
    /// <param name="scheduleMode">The schedule mode.</param>
    /// <exception cref="System.InvalidOperationException">MicroThread was already started before.</exception>
    public void Start(Func<Task> microThreadFunction, ScheduleMode scheduleMode = ScheduleMode.Last) {
        // TODO: Interlocked compare exchange?
        if (Interlocked.CompareExchange(ref state, (int)MicroThreadState.Starting, (int)MicroThreadState.None)
            != (int)MicroThreadState.None) {
            throw new InvalidOperationException("MicroThread was already started before.");
        }

        async void WrappedMicroThreadFunction() {
            try {
                State = MicroThreadState.Running;

                await microThreadFunction();

                if (State != MicroThreadState.Running) {
                    throw new InvalidOperationException("MicroThread completed in an invalid state.");
                }

                State = MicroThreadState.Completed;
            } catch (OperationCanceledException e) {
                // Exit gracefully on cancellation exceptions
                SetException(e);
            } catch (Exception e) {
                Scheduler.Log.Error("Unexpected exception while executing a micro-thread", e);
                SetException(e);
            } finally {
                lock (Scheduler.AllMicroThreads) {
                    Scheduler.AllMicroThreads.Remove(AllLinkedListNode);
                }
            }
        }

        void Callback() {
            SynchronizationContext = new MicroThreadSynchronizationContext(this);
            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

            WrappedMicroThreadFunction();
        }

        lock (Scheduler.AllMicroThreads) {
            Scheduler.AllMicroThreads.AddLast(AllLinkedListNode);
        }

        ScheduleContinuation(scheduleMode, Callback);
    }

    /// <summary>
    ///     Yields to this <see cref="MicroThread" />.
    /// </summary>
    /// <returns>Task.</returns>
    public async Task Run() {
        Reschedule(ScheduleMode.First, Priority);
        var currentScheduler = Scheduler.Current;
        if (currentScheduler == Scheduler) {
            await Scheduler.Yield();
        }
    }

    /// <summary>
    ///     Cancels the <see cref="MicroThread" />.
    /// </summary>
    public void Cancel() {
        // TODO: If we unschedule the microthread after cancellation, we never give user code the chance to throw OperationCanceledException.
        // If we don't, we can't be sure that the MicroThread ends. 
        // Should we run continuations manually?

        // Notify awaitables
        cancellationTokenSource.Cancel();

        // Unschedule microthread
        //lock (Scheduler.scheduledMicroThreads)
        //{
        //    if (ScheduledLinkedListNode.Index != -1)
        //    {
        //        Scheduler.scheduledMicroThreads.Remove(ScheduledLinkedListNode);
        //    }
        //}
    }

    MicroThreadCallbackNode NewCallback() {
        MicroThreadCallbackNode node;
        var pool = Scheduler.CallbackNodePool;

        if (Scheduler.CallbackNodePool.Count > 0) {
            var index = pool.Count - 1;
            node = pool[index];
            pool.RemoveAt(index);
        } else {
            node = new();
        }

        return node;
    }

    internal void SetException(Exception exception) {
        Exception = exception;

        // Depending on if exception was raised from outside or inside, set appropriate state
        State = exception is OperationCanceledException ? MicroThreadState.Canceled : MicroThreadState.Failed;
    }

    internal void Reschedule(ScheduleMode scheduleMode, long newPriority) {
        lock (Scheduler.ScheduledEntries) {
            if (ScheduledLinkedListNode.Index != -1) {
                Scheduler.ScheduledEntries.Remove(ScheduledLinkedListNode);
                ScheduledLinkedListNode.Value.Priority = newPriority;
                Scheduler.Schedule(ScheduledLinkedListNode, scheduleMode);
            } else {
                ScheduledLinkedListNode.Value.Priority = newPriority;
            }
        }
    }

    internal void ScheduleContinuation(ScheduleMode scheduleMode, SendOrPostCallback callback, object callbackState) {
        Debug.Assert(callback != null);
        lock (Scheduler.ScheduledEntries) {
            var node = NewCallback();
            node.SendOrPostCallback = callback;
            node.CallbackState = callbackState;
            Callbacks.Add(node);

            if (ScheduledLinkedListNode.Index == -1) {
                Scheduler.Schedule(ScheduledLinkedListNode, scheduleMode);
            }
        }
    }

    internal void ScheduleContinuation(ScheduleMode scheduleMode, Action callback) {
        Debug.Assert(callback != null);
        lock (Scheduler.ScheduledEntries) {
            var node = NewCallback();
            node.MicroThreadAction = callback;
            Callbacks.Add(node);

            if (ScheduledLinkedListNode.Index == -1) {
                Scheduler.Schedule(ScheduledLinkedListNode, scheduleMode);
            }
        }
    }
}
