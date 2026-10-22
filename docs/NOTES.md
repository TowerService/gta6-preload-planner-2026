# Developer Notes

## Overview

GTA 6 Preload Planner is a desktop storage planner for GTA 6. It helps users plan disk space before preloading the game. The utility does not download the game and does not contain any game files. It only estimates required disk space, scans available drive space, and checks files against known fake build signatures.

## Architecture

- `MainWindow.xaml` / `MainWindow.xaml.cs` — WPF UI with three tabs: Estimator, Cleanup, Fake Warning.
- `DriveScanner.cs` — Disk analyzer using DriveInfo API. Scans drives and finds largest folders. Supports async scanning with CancellationToken.
- `SizeEstimator.cs` — Size estimator for GTA 6 based on edition and platform.
- `FakeBuildDatabase.cs` — Fake build warning database with community-reported file names, size ranges, and hash prefixes.
- `PlannerConfig.cs` — JSON configuration persistence.

## Storage Estimates

Size estimates are approximate. Rockstar has not published the official GTA 6 file size. Estimates are based on publicly available information about map size and requirements from previous Rockstar titles.

- Standard Edition: 150–180 GB
- Ultimate Edition: 180–200 GB
- Preload overhead (temporary files, unpacking): +10%

## Updating fake_builds.json

The fake build database is updated manually from community reports and public safety research. To add a new entry:

1. Open `data/fake_builds.json`.
2. Add a new object with `name`, `size_range`, `hash_prefix`, and `description` fields.
3. Save the file and rebuild the project.

Hash prefixes are fictional stubs (first 8 characters only) and do not represent real harmful signatures.

## Known Limitations

- Storage estimates are approximate and may not match the final release size.
- Fake build detection relies on file names and sizes only; it cannot guarantee detection of all fake files.
- Rockstar Games has not published an official GTA 6 file size.
- The utility does not download, install, or modify game files.