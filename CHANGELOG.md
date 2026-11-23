# Changelog

Notable changes to LogViewer. Release downloads:
https://github.com/KooleControls/LogViewer/releases

## Unreleased

### Changed
- **Faster scope view.** A day of gateway logging could take around 200 ms per frame, so panning, zooming and Ctrl-hover lagged. The view now keeps its rendered data and only redraws it when the view or the data changes. Moving a marker or the hover popup no longer redraws the data, and panning and zooming draw several times faster.
- **Dense stretches are drawn simplified.** State changes too close together to tell apart (under 4 px each) are drawn as one hatched band instead of a smear of brackets. A label's text is left out where it would print over another label's text in the same row; its cross is still drawn and Ctrl-hover still lists it. Zoom in to see each change and label.

## v3.12.0

### Added
- **Heating and cooling inhibit traces.** Gateway firmware with RA2-626 logs when the heating or cooling inhibit (window contact on input 1 or 2) starts and stops blocking, after its polarity and on-delay. They show as `GIO0_HeatingInhibit` and `GIO0_CoolingInhibit` with the states **Free** and **Blocked**, next to the input and relay traces. The same firmware logs the level of both inputs after every restart, so the input traces have a value even when a contact never switches. (RA2-626)

### Changed
- **Device tab: no automatic queries on discovery.** A discovered device was immediately contacted over KC2 to read its name, version and SID, which opened a connection and handshake on every gateway that broadcast a hello. That disturbed tests and other people's devices. Each discovered device now has a **Get name** button on its line, and the device is only contacted when you click it.

### Fixed
- **Device tab: install code.** The gateway keys its KC2 encryption with its installation code, but the Device tab always used `000000`. On any other gateway it connected, yet no reply could be decrypted, so it fetched 0 lines without an error. The tab now has an **Install code** field (default `000000`, the gateway's `DIST` reply), filled in automatically when you double-click a discovered device. Discovered devices also get their name and version with their own install code.

## v3.11.0

### Changed
- **Organisation list updated** to the current production environments, the same list the KC220 config tool uses (Confluence DEV "Omgevingen"). It grows from 18 to 82 organisations. UBK and the internal Aedes / RCN servers are removed. (RA2-521)
- Resort export moved from the WebAPI tab to a dialog under the **Tools** menu.

### Added
- Live device log streaming over TCP (KC2 protocol), with automatic device discovery.

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
