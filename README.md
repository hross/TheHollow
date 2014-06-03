# The Hollow
#### A C#/XNA based mod framework for Sword of the Stars: The Pit.

[Sword of the Stars: The Pit](http://sots-thepit.com/) is an awesome game by itself, but wouldn't it be cool if you could make your own playable characters and possibly modify the game in other ways? Hopefully, now we can.

## Quick Start

Right now this process requires some knowledge of Git and C#. Maybe one of these days I can make it more plug and play:

1. Purchase [The Pit](http://sots-thepit.com/)
2. Figure out your install location. Typically it is in one of these two directories:
	* C:\Program Files (x86)\Kerberos Productions\The Pit
	* C:\Program Files (x86)\Steam\SteamApps\common\The Pit
3. Git clone this repo and download it locally.
3. Copy your `ThePit.exe` into the `original` directory in this installation.
4. Build and run `ThePit.Generator`. It should produce another `ThePit.exe` in the `lib` folder and not generate any exceptions.
5. Change ContentPath in ThePit.Launcher's App.config file to point to your pit installation (see step 2).
6. Build and run `ThePit.Launcher`.

If you see the growling Kerberos logo, you're on your way.
	
This process was tested on versions 1.2.4 and 1.2.7 of the game but I can't guarantee success. Feel free to file an issue if you run into trouble.

## Current Features

You can see a list of the current mod features [here](https://github.com/hross/TheHollow/blob/master/FEATURES.md).

## How and Why

It seemed like a fun project to try to create a mod for an awesome game. The secret sauce is the very cool [Mono.Cecil](http://www.mono-project.com/Cecil) project, which allows us to automate the inline editing of IL code (rather than having to run [ildasm](http://msdn.microsoft.com/en-us/library/f7dy01k1(v=vs.110).aspx) and manually edit IL code). Basically we decompile the original game code, figure out which classes and methods we need to override in order to inject functionality we want, and then create the hook wrappers. Finally we define some alternate data sources so we don't have to pack up XNB files and can keep things more human readable.

## Disclaimers

This mod is not affiliated with [Kerberos Productions](http://www.kerberos-productions.com/) in any way. 

Use of this mod is *at your own risk* with *absolutely no warranty*. It might break something that neither the mod author(s) nor Kerberos will fix for you. This mod may not work with future versions of the game.

Other than that, feel free to use the source and modify it to work as you see fit.