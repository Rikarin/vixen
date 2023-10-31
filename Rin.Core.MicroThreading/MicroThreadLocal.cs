using System.Runtime.CompilerServices;

namespace Rin.Core.MicroThreading;

/// <summary>
///     Provides MicroThread-local storage of data.
/// </summary>
/// <typeparam name="T">Type of data stored.</typeparam>
public class MicroThreadLocal<T> where T : class {
    readonly Func<T>? valueFactory;
    readonly ConditionalWeakTable<MicroThread, T> values = new();

    /// <summary>
    ///     The value return when we are not in a micro thread. That is the value return when
    ///     'Scheduler.CurrentMicroThread==null'
    /// </summary>
    T? valueOutOfMicroThread;

    /// <summary>
    ///     Indicate if the value out of micro-thread have been set at least once or not.
    /// </summary>
    bool valueOutOfMicroThreadSet;

    /// <summary>
    ///     Gets or sets the value for the current microthread.
    /// </summary>
    /// <value>
    ///     The value for the current microthread.
    /// </value>
    public T Value {
        get {
            T value;
            var microThread = Scheduler.CurrentMicroThread;

            lock (values) {
                if (microThread == null) {
                    if (!valueOutOfMicroThreadSet) {
                        valueOutOfMicroThread = valueFactory?.Invoke();
                    }

                    value = valueOutOfMicroThread;
                } else if (!values.TryGetValue(microThread, out value)) {
                    values.Add(microThread, value = valueFactory != null ? valueFactory() : default);
                }
            }

            return value;
        }
        set {
            var microThread = Scheduler.CurrentMicroThread;

            lock (values) {
                if (microThread == null) {
                    valueOutOfMicroThread = value;
                    valueOutOfMicroThreadSet = true;
                } else {
                    values.Remove(microThread);
                    values.Add(microThread, value);
                }
            }
        }
    }

    public bool IsValueCreated {
        get {
            var microThread = Scheduler.CurrentMicroThread;

            lock (values) {
                return microThread == null ? valueOutOfMicroThreadSet : values.TryGetValue(microThread, out _);
            }
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MicroThreadLocal{T}" /> class.
    /// </summary>
    public MicroThreadLocal()
        : this(null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MicroThreadLocal{T}" /> class.
    /// </summary>
    /// <param name="valueFactory">
    ///     The value factory invoked to create a value when <see cref="Value" /> is retrieved before
    ///     having been previously initialized.
    /// </param>
    public MicroThreadLocal(Func<T> valueFactory) {
        this.valueFactory = valueFactory;
    }

    public void ClearValue() {
        var microThread = Scheduler.CurrentMicroThread;

        lock (values) {
            if (microThread == null) {
                valueOutOfMicroThread = default;
                valueOutOfMicroThreadSet = false;
            } else {
                values.Remove(microThread);
            }
        }
    }
}
