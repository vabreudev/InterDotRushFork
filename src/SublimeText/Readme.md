# InterDotRush for Sublime Text

It provides all the features of the InterDotRush language server (including the code decompilation).

## Installation

1. Install `LSP` plugin for Sublime Text (**Package Control: Install Package** -> **LSP**).
2. Download the latest release of the InterDotRush server from [GitHub Releases](https://github.com/JaneySprings/InterDotRush/releases).
3. Open `LSP` settings file (**Preferences** -> **Package Settings** -> **LSP** -> **Settings**).
4. Create the following configuration in the `LSP` settings file:

```json
{
  "clients": {
    "InterDotRush": {
      "enabled": true,
      "command": ["Your\\Path\\To\\InterDotRush.exe"],
      "selector": "source.cs"
    }
  }
}
```

## Configuration

InterDotRush extension can be configured by creating a `InterDotRush.config.json` file in your project root directory.

You only need to provide the `projectOrSolutionFiles` option at minimum, but can customize the behavior further with additional settings as needed.

```json
{
  "InterDotRush": {
    "roslyn": {
      // Paths to project or solution files to load
      "projectOrSolutionFiles": ["/path/to/your/solution.sln"]
    }
  }
}
```

### All Configuration Options

All available configuration options can be found in the InterDotRush extension's [package.json](https://github.com/JaneySprings/InterDotRush/blob/main/package.json) file. Any option under the `InterDotRush.roslyn` namespace can be used in your settings file with the structure shown above.
