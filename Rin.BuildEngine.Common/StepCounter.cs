namespace Rin.BuildEngine.Common;

public class StepCounter {
    readonly int[] stepResults = new int[Enum.GetValues(typeof(ResultStatus)).Length];
    public int Total { get; private set; }

    public void AddStepResult(ResultStatus result) {
        lock (stepResults) {
            ++Total;
            ++stepResults[(int)result];
        }
    }

    public int Get(ResultStatus result) {
        lock (stepResults) {
            return stepResults[(int)result];
        }
    }

    public void Clear() {
        lock (stepResults) {
            Total = 0;
            foreach (var value in Enum.GetValues(typeof(ResultStatus))) {
                stepResults[(int)value] = 0;
            }
        }
    }
}
