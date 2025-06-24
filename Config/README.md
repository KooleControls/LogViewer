# LogViewer Configuration

The configuration system is designed for easy extension, overriding, and offline use.

The **main configuration file** is:

`%LOCALAPPDATA%\LogViewer\release\config.yaml`

This file is automatically created when you start the tool (if it doesn’t exist). It defines a list of **sources** YAML files containing configuration data such as organisations, profiles, and settings.

Sources can be local or remote files. You can add your own sources to extend or override the default configuration.

| Feature                   | Description                                                          |
| ------------------------- | -------------------------------------------------------------------- |
| **Merging**               | Profiles, traces, and settings are merged based on their name/type   |
| **Recursive loading**     | Config files can include other sources recursively                   |
| **Relative path support** | Paths can be relative to the including file (local or remote)        |
| **Caching**               | Remote configs are cached locally for offline use                    |
| **Schema-based editing**  | VSCode supports YAML validation and autocomplete via `schema.json`   |
| **Offline fallback**      | Uses the last cached version if the source is offline or unreachable |


# Adding a Custom Organisation

You can add your own organisations to the configuration — for example, to connect to acceptation environments or internal systems. This allows you to test or use additional environments without changing the shared configuration.

Organisations that need to be added for **all users** are managed by **Team 2**.
If you want an organisation included globally, create an issue and provide an example configuration.

## Example: add your own organisation

1️⃣ Create a YAML file with your custom organisation settings.
The location of this file does not matter, but a good example is:

`%LOCALAPPDATA%\LogViewer\user\organisations.yaml`

Example content:

```yaml
organisations:
  my_acceptation_env:
    name: My Acceptation Environment
    organisationId: 42
    basePath: https://acceptation.mycompany.com/api
    authenticationMethod: GetOAuth2_OpenIdConnectClient
    authPath: https://accounts.mycompany.com/realms/acceptation
```

2️⃣ Update your main configuration file so that your custom file is loaded:

`%LOCALAPPDATA%\LogViewer\release\config.yaml`

```yaml
sources:
  - https://raw.githubusercontent.com/KooleControls/LogViewerConfig/Releases/%APPVERSION%/Config/Sources.yaml
  - "%LOCALAPPDATA%/LogViewer/user/organisations.yaml"
```

This will load your custom organisation alongside the standard configuration.
