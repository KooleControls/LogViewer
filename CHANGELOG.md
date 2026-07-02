# Changelog

Notable changes to LogViewer. Release downloads:
https://github.com/KooleControls/LogViewer/releases

## v3.10.0

KC220 config tool integration — export live API data for the KC220 config tool and open the tool directly on a device.

### Added
- **Export resort** (button next to the resort dropdown): exports the selected resort — its connection-server settings (server address, ComPort/TrgPort, install code) and all its devices (objects → gateways, with Software ID / Device ID) — to a `.kcresort` file that the KC220 config tool can import. Shows progress; the button becomes **Cancel** while it runs.
- **Export all** (button next to the organisation dropdown): exports **every** resort of the selected organisation to a single `.kcbundle` file (all resorts / objects / devices in one file), so the KC220 tool can import a whole organisation in one action instead of exporting resorts one by one.
- **220 tool** (button next to the gateway dropdown): opens the KC220 config tool straight onto the selected device — it writes a temporary `.kcresort` and launches `KC220 Config tool.exe /connect "<file>"`. The KC220 executable path is asked once and remembered.
- Confirmation dialogs on the export buttons warning that they make many web-API calls and can take a while (a full organisation can take several minutes).

### Notes
- The device export carries the object name (Concern → Resort → Object → Device), matching the KC220 tool's hierarchy.
- The web API does not expose the device encryption key, so exported files leave it blank (it does not affect connecting to the device).
