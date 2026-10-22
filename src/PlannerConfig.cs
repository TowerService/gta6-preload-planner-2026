using System;
using System.IO;
using System.Text.Json;

namespace GTA6PreloadPlanner
{
    /// <summary>
    /// Represents planner configuration persisted to JSON.
    /// </summary>
    public class PlannerConfig
    {
        public string SelectedDrive { get; set; } = string.Empty;
        public string Edition { get; set; } = "Standard";
        public string Platform { get; set; } = "PC";

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GTA6PreloadPlanner",
            "config.json");

        /// <summary>
        /// Loads configuration from disk. Returns default values if file does not exist.
        /// </summary>
        public static PlannerConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new PlannerConfig();

                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<PlannerConfig>(json) ?? new PlannerConfig();
            }
            catch
            {
                return new PlannerConfig();
            }
        }

        /// <summary>
        /// Saves configuration to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // Silently ignore save failures
            }
        }
    }
}