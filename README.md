# Status client for Windows
https://github.com/BotoX/ServerStatus client implementation in C# for Windows

[![wakatime](https://wakatime.com/badge/github/filip2cz/status-client-csharp.svg?8)](https://wakatime.com/badge/github/filip2cz/status-client-csharp)

## Installation

You can create something like service in Task Scheduler, where you should create new task and propably choose "run at startup", if you want to run it automatically after computer reboot.

## Configuration
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
