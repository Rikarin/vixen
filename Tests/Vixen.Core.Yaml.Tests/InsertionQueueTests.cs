using Xunit;

namespace Vixen.Core.Yaml.Tests;

public class InsertionQueueTests {
    [Fact]
    public void ShouldThrowExceptionWhenDequeuingEmptyContainer() {
        var queue = CreateQueue();

        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void ShouldThrowExceptionWhenDequeuingContainerThatBecomesEmpty() {
        var queue = new InsertionQueue<int>();

        queue.Enqueue(1);
        queue.Dequeue();

        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void ShouldCorrectlyDequeueElementsAfterEnqueuing() {
        var queue = CreateQueue();

        WithTheRange(0, 10).Perform(queue.Enqueue);

        Assert.Equal(
            new() {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9
            },
            OrderOfElementsIn(queue).ToList()
        );
    }

    [Fact]
    public void ShouldCorrectlyDequeueElementsWhenIntermixingEnqueuing() {
        var queue = CreateQueue();

        WithTheRange(0, 10).Perform(queue.Enqueue);
        PerformTimes(5, queue.Dequeue);
        WithTheRange(10, 15).Perform(queue.Enqueue);

        Assert.Equal(
            new() {
                5,
                6,
                7,
                8,
                9,
                10,
                11,
                12,
                13,
                14
            },
            OrderOfElementsIn(queue).ToList()
        );
    }

    [Fact]
    public void ShouldThrowExceptionWhenDequeuingAfterInserting() {
        var queue = CreateQueue();

        queue.Enqueue(1);
        queue.Insert(0, 99);
        PerformTimes(2, queue.Dequeue);

        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void ShouldCorrectlyDequeueElementsWhenInserting() {
        var queue = CreateQueue();

        WithTheRange(0, 10).Perform(queue.Enqueue);
        queue.Insert(5, 99);

        Assert.Equal(
            new() {
                0,
                1,
                2,
                3,
                4,
                99,
                5,
                6,
                7,
                8,
                9
            },
            OrderOfElementsIn(queue).ToList()
        );
    }

    static InsertionQueue<int> CreateQueue() => new();

    IEnumerable<int> WithTheRange(int from, int to) => Enumerable.Range(from, to - from);

    IEnumerable<int> OrderOfElementsIn(InsertionQueue<int> queue) {
        while (true) {
            if (queue.Count == 0) {
                yield break;
            }

            yield return queue.Dequeue();
        }
    }

    void PerformTimes(int times, Func<int> func) {
        WithTheRange(0, times).Perform(func);
    }
}

public static class EnumerableExtensions {
    public static void Perform<T>(this IEnumerable<T> withRange, Func<int> func) {
        withRange.Perform(x => func());
    }

    public static void Perform<T>(this IEnumerable<T> withRange, Action<T> action) {
        foreach (var element in withRange) {
            action(element);
        }
    }
}
