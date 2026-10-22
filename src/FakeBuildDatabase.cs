using System;
using System.Collections.Generic;
using System.Linq;

namespace GTA6PreloadPlanner
{
    /// <summary>
    /// Represents a known fake build entry from community reports.
    /// </summary>
    public class FakeBuildEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SizeRange { get; set; } = string.Empty;
        public string HashPrefix { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Database of known fake GTA 6 build files.
    /// </summary>
    public class FakeBuildDatabase
    {
        private readonly List<FakeBuildEntry> _entries;

        /// <summary>
        /// Initializes the database with community-reported entries.
        /// Database is populated from community reports and security research. Update manually.
        /// </summary>
        public FakeBuildDatabase()
        {
            _entries = new List<FakeBuildEntry>
            {
                new FakeBuildEntry
                {
                    Name = "GTA6_Setup.exe",
                    SizeRange = "100-120 GB",
                    HashPrefix = "a3f9c2e1",
                    Description = "Reported harmful wrapper — small payload with empty padding"
                },
                new FakeBuildEntry
                {
                    Name = "GTA6_Leak_113GB.rar",
                    SizeRange = "110-115 GB",
                    HashPrefix = "7d4e1b9a",
                    Description = "Reported harmful content disguised as game archive"
                }
            };
        }

        /// <summary>
        /// Checks whether a file matches a known fake build entry.
        /// </summary>
        /// <param name="fileName">File name to check.</param>
        /// <param name="fileSize">File size in bytes.</param>
        /// <returns>True if the file matches a known fake entry.</returns>
        public bool IsKnownFake(string fileName, long fileSize)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var normalizedName = fileName.ToLowerInvariant();

            foreach (var entry in _entries)
            {
                var entryName = entry.Name.ToLowerInvariant();
                if (!normalizedName.Contains(entryName))
                    continue;

                if (MatchesSizeRange(entry.SizeRange, fileSize))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all fake build entries.
        /// </summary>
        public List<FakeBuildEntry> GetAllEntries() => _entries;

        private static bool MatchesSizeRange(string sizeRange, long fileSizeBytes)
        {
            try
            {
                var parts = sizeRange.Split('-');
                if (parts.Length != 2)
                    return false;

                var minGB = double.Parse(parts[0].Trim().Replace("GB", ""));
                var maxGB = double.Parse(parts[1].Trim().Replace("GB", ""));
                var sizeGB = fileSizeBytes / (1024.0 * 1024.0 * 1024.0);

                return sizeGB >= minGB && sizeGB <= maxGB;
            }
            catch
            {
                return false;
            }
        }
    }
}