using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Vixen.Core.IO;

public static class NativeLockFile {
    internal const uint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
    internal const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

    public static void LockFile(FileStream fileStream, long offset, long count, bool exclusive) {
        if (Platform.Type == PlatformType.Android) {
            // Android does not support large file and thus is limited to files
            // whose sizes are less than 2GB.
            // We subtract the offset to not go beyond the 2GB limit.
            count = count + offset > int.MaxValue ? int.MaxValue - offset : count;
        }

        if (Platform.Type == PlatformType.Windows) {
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new NativeOverlapped {
                InternalLow = IntPtr.Zero,
                InternalHigh = IntPtr.Zero,
                OffsetLow = (int)(offset & 0x00000000FFFFFFFF),
                OffsetHigh = (int)(offset >> 32),
                EventHandle = IntPtr.Zero
            };

            if (!LockFileEx(
                    fileStream.SafeFileHandle,
                    exclusive ? LOCKFILE_EXCLUSIVE_LOCK : 0,
                    0,
                    countLow,
                    countHigh,
                    ref overlapped
                )) {
                throw new IOException("Couldn't lock file.");
            }
        } else {
            bool tryAgain;
            do {
                tryAgain = false;
                try {
                    fileStream.Lock(offset, count);
                } catch (IOException) {
                    tryAgain = true;
                }
            } while (tryAgain);
        }
    }

    public static void UnlockFile(FileStream fileStream, long offset, long count) {
        if (Platform.Type == PlatformType.Android) {
            // See comment on `LockFile`.
            count = count + offset > int.MaxValue ? int.MaxValue - offset : count;
        }

        if (Platform.Type == PlatformType.Windows) {
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new NativeOverlapped {
                InternalLow = IntPtr.Zero,
                InternalHigh = IntPtr.Zero,
                OffsetLow = (int)(offset & 0x00000000FFFFFFFF),
                OffsetHigh = (int)(offset >> 32),
                EventHandle = IntPtr.Zero
            };

            if (!UnlockFileEx(fileStream.SafeFileHandle, 0, countLow, countHigh, ref overlapped)) {
                throw new IOException("Couldn't unlock file.");
            }
        } else {
            fileStream.Unlock(offset, count);
        }
    }

    [DllImport("Kernel32.dll", SetLastError = true)]
    internal static extern bool LockFileEx(
        SafeFileHandle handle,
        uint flags,
        uint reserved,
        uint countLow,
        uint countHigh,
        ref NativeOverlapped overlapped
    );

    [DllImport("Kernel32.dll", SetLastError = true)]
    internal static extern bool UnlockFileEx(
        SafeFileHandle handle,
        uint reserved,
        uint countLow,
        uint countHigh,
        ref NativeOverlapped overlapped
    );
}
