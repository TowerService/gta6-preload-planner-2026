using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GTA6PreloadPlanner
{
    /// <summary>
    /// Main window for GTA 6 Preload Planner.
    /// Handles storage estimation, drive scanning, and fake build warnings.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DriveScanner _driveScanner = new DriveScanner();
        private readonly SizeEstimator _sizeEstimator = new SizeEstimator();
        private readonly FakeBuildDatabase _fakeBuildDatabase = new FakeBuildDatabase();
        private PlannerConfig _config;
        private CancellationTokenSource? _scanCancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            _config = PlannerConfig.Load();
            ApplyConfigToUI();

            LoadDrives();
            LoadFakeBuilds();

            UpdateStatus("Ready");
        }

        private void ApplyConfigToUI()
        {
            if (!string.IsNullOrEmpty(_config.SelectedDrive))
                cmbDrive.SelectedItem = cmbDrive.Items.Cast<ComboBoxItemWrapper>()
                    .FirstOrDefault(i => i.Text == _config.SelectedDrive);

            cmbEdition.SelectedIndex = _config.Edition == "Ultimate" ? 1 : 0;
            cmbPlatform.SelectedIndex = 0; // PC default
        }

        private void LoadDrives()
        {
            var drives = _driveScanner.GetDrives();
            cmbDrive.Items.Clear();
            cmbCleanupDrive.Items.Clear();

            foreach (var drive in drives)
            {
                var display = $"{drive.Name} ({drive.AvailableSpaceGB} GB free of {drive.TotalSizeGB} GB)";
                cmbDrive.Items.Add(new ComboBoxItemWrapper { Text = display, Value = drive.Name });
                cmbCleanupDrive.Items.Add(new ComboBoxItemWrapper { Text = display, Value = drive.Name });
            }
        }

        private void LoadFakeBuilds()
        {
            var entries = _fakeBuildDatabase.GetAllEntries();
            lstFakeBuilds.Items.Clear();
            foreach (var entry in entries)
            {
                lstFakeBuilds.Items.Add(
                    $"{entry.Name} — {entry.SizeRange} — Hash: {entry.HashPrefix}\n  {entry.Description}");
            }
        }

        private void UpdateStatus(string message)
        {
            txtStatus.Text = message;
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            var selected = cmbDrive.SelectedItem as ComboBoxItemWrapper;
            if (selected == null)
            {
                UpdateStatus("Please select a drive");
                return;
            }

            var edition = cmbEdition.SelectedItem is ComboBoxItemWrapper ei && ei.Text.StartsWith("Ultimate")
                ? SizeEstimator.Edition.Ultimate
                : SizeEstimator.Edition.Standard;

            var estimate = _sizeEstimator.GetEstimate(edition, SizeEstimator.Platform.PC);
            var available = _driveScanner.GetAvailableSpace(selected.Value);

            var fits = available >= estimate.MaxGB;

            txtEstimateResult.Text =
                $"Edition: {edition}\n" +
                $"Estimated size: {estimate.MinGB}–{estimate.MaxGB} GB (including preload overhead)\n" +
                $"Available on {selected.Value}: {available} GB\n" +
                $"{(fits ? "Enough space available" : "Not enough space — free up disk space")}";

            UpdateStatus("Estimate calculated");

            _config.SelectedDrive = selected.Value;
            _config.Edition = edition.ToString();
            _config.Save();
        }

        private void BtnScanCleanup_Click(object sender, RoutedEventArgs e)
        {
            var selected = cmbCleanupDrive.SelectedItem as ComboBoxItemWrapper;
            if (selected == null)
            {
                UpdateStatus("Please select a drive");
                return;
            }

            _scanCancellationTokenSource = new CancellationTokenSource();
            prgScan.Visibility = Visibility.Visible;
            lstLargeFolders.Items.Clear();

            _ = Task.Run(async () =>
            {
                try
                {
                    var folders = await _driveScanner.GetLargestFolders(selected.Value, 10, _scanCancellationTokenSource.Token);

                    Dispatcher.Invoke(() =>
                    {
                        prgScan.Visibility = Visibility.Collapsed;
                        foreach (var folder in folders)
                            lstLargeFolders.Items.Add($"{folder.Path} — {folder.SizeGB} GB");
                        UpdateStatus($"Scan complete — {folders.Count} folders found");
                    });
                }
                catch (OperationCanceledException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        prgScan.Visibility = Visibility.Collapsed;
                        UpdateStatus("Scan cancelled");
                    });
                }
            });
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstLargeFolders.SelectedItem as string;
            if (string.IsNullOrEmpty(selected))
                return;

            var path = selected.Split('—')[0].Trim();
            if (Directory.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe", path);
        }

        protected override void OnClosed(EventArgs e)
        {
            _scanCancellationTokenSource?.Cancel();
            _scanCancellationTokenSource?.Dispose();
            base.OnClosed(e);
        }

        private class ComboBoxItemWrapper
        {
            public string Text { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public override string ToString() => Text;
        }
    }
}