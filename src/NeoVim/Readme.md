# InterDotRush for Neovim

It provides all the features of the InterDotRush language server (including the code decompilation).

## Installation

1. Ensure you have a plugin manager like [vimplug](https://github.com/junegunn/vim-plug) installed.
2. Download the `InterDotRush.Server` executable from the latest [GitHub release](https://github.com/JaneySprings/InterDotRush/releases/latest) and place it in any location on your computer.
3. Add the following lines to your init.vim configuration:

```lua
call plug#begin()
Plug 'https://github.com/neovim/nvim-lspconfig'
call plug#end()

lua << EOF
require('lspconfig.configs').InterDotRush = {
    default_config = {
        cmd = { '/Path/To/Executale/InterDotRush' }, -- Adjust path to the InterDotRush executable
        filetypes = { 'cs', 'xaml' },
        root_dir = function(fname)
            return vim.fn.getcwd()
        end
    };
}
require('lspconfig').InterDotRush.setup({})
EOF

```

![image](https://github.com/JaneySprings/InterDotRush/raw/main/assets/image6.jpg)

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
