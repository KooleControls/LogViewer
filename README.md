# LogViewer

A powerful Windows Forms application for viewing, analyzing, and managing log data with advanced configuration and API integration capabilities.

## Features

- **Log Viewing & Analysis**: Advanced log viewing interface with scope-based visualization
- **Multiple Format Support**: Import and view logs in various formats including CSV and YAML
- **Configurable Data Sources**: Connect to different API endpoints and data sources
- **YAML Configuration System**: Flexible configuration with support for organizations, profiles, and settings
- **Caching System**: Built-in SQLite-based caching for improved performance and offline usage
- **Trace & Marker Support**: Advanced log tracing and marking capabilities
- **Real-time Updates**: Dynamic log viewing with real-time data updates

## System Requirements

- **Operating System**: Windows 7 or later
- **.NET Runtime**: .NET 8.0 or later
- **Architecture**: Any CPU (x86/x64)

## Installation

### Download Pre-built Release

1. Go to the [Releases](https://github.com/KooleControls/LogViewer/releases) page
2. Download the latest `LogViewer_Portable_vX.X.X.zip` file
3. Extract the archive to your desired location
4. Run `LogViewer.exe`

### Build from Source

#### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or Visual Studio Build Tools
- Access to KooleControls Azure DevOps NuGet feed (for dependencies)

#### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/KooleControls/LogViewer.git
   cd LogViewer
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build --configuration Release
   ```

4. Run the application:
   ```bash
   dotnet run --project LogViewer
   ```

## Configuration

LogViewer uses a powerful YAML-based configuration system that supports:

- **Multiple Sources**: Local and remote configuration files
- **Organizations**: Connect to different API environments
- **Profiles**: Customizable log viewing profiles
- **Offline Caching**: Automatic caching for offline usage
- **Extensible Settings**: Easy to extend and customize

### Configuration Location

The main configuration file is located at:
```
%LOCALAPPDATA%\LogViewer\release\config.yaml
```

For development builds:
```
%LOCALAPPDATA%\LogViewer\debug\config.yaml
```

### Adding Custom Organizations

You can extend the configuration to include custom organizations or environments. See the [Configuration Guide](Config/README.md) for detailed instructions on:

- Adding custom organizations
- Setting up acceptation environments
- Configuring authentication methods
- Managing configuration sources

## Usage

1. **Launch the Application**: Start LogViewer.exe
2. **Select Data Source**: Use the API Source Control to select your log data source
3. **Configure Connection**: Set up your organization and authentication if needed
4. **View Logs**: Browse and analyze your log data using the various viewing modes:
   - **Scope View**: Hierarchical log structure visualization
   - **Marker View**: Important log markers and annotations
   - **Trace View**: Detailed trace analysis

## Project Structure

```
LogViewer/
├── LogViewer/           # Main Windows Forms application
├── NET_Library/         # Supporting libraries
│   ├── CoreLib/         # Core functionality
│   └── FormsLib/        # UI components and controls
├── Config/              # Configuration files and documentation
└── .github/workflows/   # CI/CD pipeline configuration
```

## Development

### Core Technologies

- **.NET 8**: Target framework
- **Windows Forms**: UI framework
- **YamlDotNet**: YAML configuration parsing
- **SQLite**: Local caching and storage
- **Octokit**: GitHub integration
- **Microsoft.Extensions**: Dependency injection and caching

### Architecture

The application follows a modular architecture with:

- **Dependency Injection**: Service-based architecture using Microsoft.Extensions.DependencyInjection
- **Configuration Service**: Centralized configuration management
- **Scope Controller**: Log visualization and control logic
- **Data Providers**: Pluggable data source providers
- **Caching Layer**: Hybrid caching for performance optimization

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Build and test locally
5. Submit a pull request

## License

This project is developed by KooleControls. Please refer to the license terms provided by the organization.

## Support

For support and questions, please:

1. Check the [Configuration Guide](Config/README.md) for setup issues
2. Review existing [Issues](https://github.com/KooleControls/LogViewer/issues)
3. Create a new issue with detailed information about your problem

## Version Information

Current version: 3.5.8.0

The application automatically updates version information during the build process and supports multiple build configurations (Debug, Release, Demo).