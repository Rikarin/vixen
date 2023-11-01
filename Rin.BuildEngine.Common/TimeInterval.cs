namespace Rin.BuildEngine.Common;

/// <summary>
///     An helper class used to store command timing
/// </summary>
public class TimeInterval {
    const long IntervalNotEnded = long.MaxValue;
    public long StartTime { get; private set; }
    public long EndTime { get; private set; } = IntervalNotEnded;

    public bool HasEnded => EndTime != IntervalNotEnded;

    public TimeInterval(long startTime) {
        StartTime = startTime;
    }

    public TimeInterval(long startTime, long endTime) {
        StartTime = startTime;
        EndTime = endTime;
    }

    public void End(long endTime) {
        if (EndTime != IntervalNotEnded) {
            throw new InvalidOperationException("TimeInterval has already ended");
        }

        EndTime = endTime;
    }

    public bool Overlap(long startTime, long endTime) =>
        (StartTime > startTime ? StartTime : startTime) < (EndTime < endTime ? EndTime : endTime);
}

public class TimeInterval<T> : TimeInterval {
    public T Object { get; protected set; }

    public TimeInterval(T obj, long startTime)
        : base(startTime) {
        Object = obj;
    }

    public TimeInterval(T obj, long startTime, long endTime)
        : base(startTime, endTime) {
        Object = obj;
    }
}
