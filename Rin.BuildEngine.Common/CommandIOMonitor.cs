using Rin.Core.Serialization.Serialization.Contents;
using Rin.Core.Storage;
using Serilog;
using System.Diagnostics;

namespace Rin.BuildEngine.Common;

/// <summary>
///     This class monitors input/output access from every BuildStep execution, and display an error message if an object
///     url is the input of a command and the output of another command running at the same time.
/// </summary>
class CommandIOMonitor {
    /// <summary>
    ///     A dictionary containing read and write access timings (value) of a given object url (key)
    /// </summary>
    readonly Dictionary<ObjectUrl, ObjectAccesses> objectsAccesses = new();

    /// <summary>
    ///     A dictionary containing execution intervals of BuildStep
    /// </summary>
    readonly Dictionary<CommandBuildStep, TimeInterval> commandExecutionIntervals = new();

    readonly Dictionary<CommandBuildStep, List<ObjectUrl>> commandInputFiles = new();

    readonly ILogger logger;

    readonly object lockObject = new();

    readonly Stopwatch stopWatch = new();

    // Store earliest start time of command still running (to clean up accesses as time goes)
    long earliestCommandAliveStartTime;

    public CommandIOMonitor(ILogger logger) {
        this.logger = logger;
        stopWatch.Start();
    }

    public void CommandStarted(CommandBuildStep command) {
        lock (lockObject) {
            var startTime = stopWatch.ElapsedTicks;
            commandExecutionIntervals.Add(command, new TimeInterval(startTime));

            // Get a list of unique input files
            var inputFiles = command.Command.GetInputFiles().Distinct().ToList();
            // Store it aside, so that we're sure to remove the same entries during CommandEnded
            commandInputFiles.Add(command, inputFiles);

            // Setup start read time for each file entry
            var inputHash = new HashSet<ObjectUrl>();
            foreach (var inputUrl in inputFiles) {
                if (inputHash.Contains(inputUrl)) {
                    logger.Error(
                        $"The command '{command.Title}' has several times the file '{inputUrl.Path}' as input. Input Files must not be duplicated"
                    );
                }

                inputHash.Add(inputUrl);

                ObjectAccesses inputAccesses;
                if (!objectsAccesses.TryGetValue(inputUrl, out inputAccesses)) {
                    objectsAccesses.Add(inputUrl, inputAccesses = new());
                }

                inputAccesses.Reads.Add(new TimeInterval<BuildStep>(command, startTime));
            }
        }
    }

    public void CommandEnded(CommandBuildStep command) {
        lock (lockObject) {
            TimeInterval commandInterval = commandExecutionIntervals[command];

            long startTime = commandInterval.StartTime;
            var endTime = stopWatch.ElapsedTicks;
            commandInterval.End(endTime);

            commandExecutionIntervals.Remove(command);

            foreach (var outputObject in command.Result.OutputObjects) {
                var outputUrl = outputObject.Key;
                if (objectsAccesses.TryGetValue(outputUrl, out var inputAccess)) {
                    foreach (TimeInterval<BuildStep> input in inputAccess.Reads.Where(
                                 input => input.Object != command && input.Overlap(startTime, endTime)
                             )) {
                        logger.Error(
                            $"Command {command} is writing {outputUrl} while command {input.Object} is reading it"
                        );
                    }
                }

                if (!objectsAccesses.TryGetValue(outputUrl, out var outputAccess)) {
                    objectsAccesses.Add(outputUrl, outputAccess = new());
                }

                foreach (var output in outputAccess.Writes.Where(
                             output => output.Object.Key != command && output.Overlap(startTime, endTime)
                         )) {
                    if (outputObject.Value != output.Object.Value) {
                        logger.Error(
                            $"Commands {command} and {output.Object} are both writing {outputUrl} at the same time, but they are different objects"
                        );
                    }
                }

                outputAccess.Writes.Add(
                    new TimeInterval<KeyValuePair<BuildStep, ObjectId>>(
                        new KeyValuePair<BuildStep, ObjectId>(command, outputObject.Value),
                        startTime,
                        endTime
                    )
                );
            }

            foreach (var inputUrl in command.Result.InputDependencyVersions.Keys) {
                if (objectsAccesses.TryGetValue(inputUrl, out var outputAccess)) {
                    foreach (TimeInterval<KeyValuePair<BuildStep, ObjectId>> output in outputAccess.Writes.Where(
                                 output => output.Object.Key != command && output.Overlap(startTime, endTime)
                             )) {
                        logger.Error(
                            $"Command {output.Object} is writing {inputUrl} while command {command} is reading it"
                        );
                    }
                }
            }

            // Notify that we're done reading input files
            if (commandInputFiles.TryGetValue(command, out var inputFiles)) {
                commandInputFiles.Remove(command);
                foreach (var input in inputFiles) {
                    objectsAccesses[input].Reads.Single(x => x.Object == command).End(endTime);
                }
            }

            // "Garbage collection" of accesses
            var newEarliestCommandAliveStartTime = commandExecutionIntervals.Count > 0
                ? commandExecutionIntervals.Min(x => x.Value.StartTime)
                : endTime;
            if (newEarliestCommandAliveStartTime > earliestCommandAliveStartTime) {
                earliestCommandAliveStartTime = newEarliestCommandAliveStartTime;

                // We can remove objects whose all R/W accesses are "completed" (EndTime is set)
                // and happened before all the current running commands started, since they won't affect us
                foreach (var objectAccesses in objectsAccesses.ToList()) {
                    if (objectAccesses.Value.Reads.All(x => x.EndTime != 0 && x.EndTime < earliestCommandAliveStartTime)
                        && objectAccesses.Value.Writes.All(
                            x => x.EndTime != 0 && x.EndTime < earliestCommandAliveStartTime
                        )) {
                        objectsAccesses.Remove(objectAccesses.Key);
                    }
                }
            }
        }
    }

    class ObjectAccesses {
        public List<TimeInterval<BuildStep>> Reads { get; } = new();
        public List<TimeInterval<KeyValuePair<BuildStep, ObjectId>>> Writes { get; } = new();
    }
}
