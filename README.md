# Status client for Windows
https://github.com/BotoX/ServerStatus client implementation in C# for Windows

[![wakatime](https://wakatime.com/badge/github/filip2cz/status-client-csharp.svg?8)](https://wakatime.com/badge/github/filip2cz/status-client-csharp)

## Installation

### Package manager - [Scoop](https://scoop.sh)
```
scoop bucket add henshouse https://github.com/henshouse/henshouse-scoop

scoop install henshouse/status-client-csharp
```
Now 
you can run it with `status-client-csharp`

### Installer
Download and run installer from github [releases](https://github.com/filip2cz/status-client-csharp/releases)

### Portable
Download .zip file from [releases](https://github.com/filip2cz/status-client-csharp/releases) and unzip it wherever you want.

## Autorun
You can create something like service in Task Scheduler, where you should create new task and propably choose "run at startup", if you want to run it automatically after computer reboot.

## Configuration

##### Config files are in the folder where you execute it!
Configuration is in config.json file.

example config file:
```
{
  "server": "example.com",
  "port": 35601,
  "user": "user",
  "passwd": "password",
  "debug": false
}
``` 
