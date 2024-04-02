using Serilog;

namespace Vixen.Editor;

public class MetadataManager {
    public static void CreateMetadata(string fullPath) {
        fullPath += ".meta";

        if (File.Exists(fullPath)) {
            return;
        }

        using var stream = File.CreateText(fullPath);
        stream.WriteLine("Meta TODO");
    }

    public static void RenameMetadata(string oldFullPath, string newFullPath) {
        try {
            File.Move($"{oldFullPath}.meta", $"{newFullPath}.meta");
        } catch (FileNotFoundException e) {
            Log.Error("Metadata::Rename: File {FileName} not found", e.FileName);
        }
    }

    public static Metadata GetMetadata(string fullPath) =>
        // TODO: We should be able to update the Metadata class when file is updated
        // Also change Metadata.Path when user moves the file
        new(fullPath);
}
