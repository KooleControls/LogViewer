# LogViewer

LogViewer is a Windows Forms application for viewing and analyzing log data.

## Configuration

LogViewer uses YAML to define available organizations and environments.

* **Built-in organizations** are managed in source and are always available.
* **Custom organizations** can be added by the user via a local configuration file.

### Configuration Location

```
%LOCALAPPDATA%\LogViewer\user_organisations.yaml
```

For development builds:

```
%LOCALAPPDATA%\LogViewer\LogViewer_debug\user_organisations.yaml
```

### Adding Custom Organizations

1. Open `user_organisations.yaml`
2. An example organization is already included
3. Copy the example and adjust the values to match your environment

## Installation

1. Download the latest portable release from the
   [https://github.com/KooleControls/LogViewer/releases](https://github.com/KooleControls/LogViewer/releases)
2. Extract the ZIP archive
3. Run `LogViewer.exe`

## License

Developed by KooleControls. See the license file for details.

## Support

* Check existing issues: [https://github.com/KooleControls/LogViewer/issues](https://github.com/KooleControls/LogViewer/issues)
* Open a new issue if needed, including relevant details
