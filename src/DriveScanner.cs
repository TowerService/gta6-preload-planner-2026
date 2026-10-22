using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTA6PreloadPlanner
{
    /// <summary>
    /// Provides drive scanning functionality for the GTA 6 Preload Planner.
    /// </summary>
    public class DriveScanner
    {
        /// <summary>
        /// Represents information about a drive.
        /// </summary>
        public class DriveInfoItem
        {
            public string Name { get; set; } = string.Empty;
            public string DriveType { get; set; } = string.Empty;
            public long TotalSizeGB { get; set; }
            public long AvailableSpaceGB { get; set; }
            public bool IsReady { get; set; }
        }

        /// <summary>
        /// Represents a folder with its size.
        /// </summary>
        public class FolderSize
        {
            public string Path { get; set; } = string.Empty;
            public long SizeGB { get; set; }
        }

        /// <summary>
        /// Gets all ready drives on the system.
        /// </summary>
        public List<DriveInfoItem> GetDrives()
        {
            var drives = new List<DriveInfoItem>();
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (!drive.IsReady)
                    continue;

                drives.Add(new DriveInfoItem
                {
                    Name = drive.Name,
                    DriveType = drive.DriveType.ToString(),
                    TotalSizeGB = drive.TotalSize / (1024L * 1024 * 1024),
                    AvailableSpaceGB = drive.AvailableFreeSpace / (1024L * 1024 * 1024),
                    IsReady = true
                });
            }
            return drives;
        }

        /// <summary>
        /// Gets available free space in GB for a drive letter.
        /// </summary>
        /// <param name="driveLetter">Drive letter, e.g. "C:\\".</param>
        public long GetAvailableSpace(string driveLetter)
        {
            try
            {
                var drive = new System.IO.DriveInfo(driveLetter);
                if (!drive.IsReady)
                    return 0;
                return drive.AvailableFreeSpace / (1024L * 1024 * 1024);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Finds the largest folders under a path.
        /// </summary>
        /// <param name="path">Root path to scan.</param>
        /// <param name="count">Maximum number of folders to return.</param>
        /// <param name="cancellationToken">Token to cancel scanning.</param>
        public async Task<List<FolderSize>> GetLargestFolders(string path, int count, CancellationToken cancellationToken = default)
        {
            var folderSizes = new List<FolderSize>();

            try
            {
                var directories = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
                foreach (var dir in directories)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        long size = DirectorySize(dirInfo);
                        folderSizes.Add(new FolderSize
                        {
                            Path = dir,
                            SizeGB = size / (1024L * 1024 * 1024)
                        });
                    }
                    catch
                    {
                        // Skip inaccessible directories
                    }
                }
            }
            catch
            {
                // Return what we have
            }

            return folderSizes
                .OrderByDescending(f => f.SizeGB)
                .Take(count)
                .ToList();
        }

        private static long DirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            try
            {
                foreach (var file in dir.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    try { size += file.Length; } catch { }
                }
                foreach (var subdir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    try { size += DirectorySize(subdir); } catch { }
                }
            }
            catch { }
            return size;
        }
    }
}