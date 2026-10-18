# WaterStrap for macOS — Preview

This is a new Avalonia-based macOS app project, separate from the current Windows-only WPF bootstrapper.

Current preview functionality:
- Open Roblox home and the official download page.
- Open a Roblox experience from a numeric place ID or a Roblox game URL.
- Save and relaunch up to eight recent place IDs locally.
- Store recent games in `~/Library/Application Support/WaterStrap/recent-games.json` on macOS.

Not yet ported from the Windows project:
- Roblox installation/bootstrapper and updater internals.
- Windows Registry and shortcut integration.
- Windows-only FastFlag/mod/cursor/system integration.
- Windows-specific notification area and process-launch logic.

The Mac launcher delegates to Roblox's official website/client flow. It does not claim full Bloxstrap feature parity.
