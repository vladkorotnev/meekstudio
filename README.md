# MikuASM + MeekStudio

This is a software toolkit designed for editing game engine script binaries in a human-readable way similar to an assembly language.

## Syntax

Overall MikuASM syntax is: `COMMAND arg1, arg2, ...`. Every command goes on a new line. Pretty much your standard assembler syntax, huh?

Shortcuts are available for some commands:

| Command | Shortcut | Example  |
| ------- | -------- | -------- |
| `TIME`  | `@`      | `@ 1000` |
| `END`   | `.`      |          |
| `PV_END` | `$`     | `$.`     |

Comments can be made either by C-style `// comment` or Lua-style `-- comment`.

### TIME command format

To make writing scripts easier, it is possible to specify the `TIME` (`@`) command argument in the following forms:

* `ms` e.g. `120420`
* `s.ms` e.g. `120.420`
* `M:s.ms` e.g. `2:00.420`
* `Fframe` e.g. `F6599` (only approximate at roughly 58.842 FPS, not recommended to use)

Some other commands also allow using proper `true`/`false` instead of `0`/`1` where appropriate, or use a percentage value to reference against an experimentally determined in-game maximum rather than having to use absolute values.

## Preprocessor directives

### `#include path/to/.mia`

Includes a source code text file in place. This does not create a graph entry like C-style compilers, but simply replaces the line with the contents of the file.

Example: `#include scenes/00_Intro/01_Bridge.mia`

### `#incbin path/to/.dsc`

Includes a precompiled DSC binary file in place. This does not create a graph entry like C-style compilers, but simply replaces the line with the contents of the file. 

This command is affected by the `BINFLT` setting.

Example: `#include chart/00_Easy/exported_data/Easy.dsc`

### `#binflt ONLY_CHART|WITHOUT_CHART|NO_FILTER`

Enables or disables filtering of the included binary files. If set to `ONLY_CHART`, only the following commands are loaded from `#incbin` directives (`WITHOUT_CHART` is vice-versa to an extent):

* `TIME`
* `TARGET`
* `TARGET_FLYING_TIME`
* `END`
* `PV_END`
* `MUSIC_PLAY`

This feature may be useful when using the `#incbin` directive to include files containing only the target grid, created using external GUI editors.

Example: `#binflt ONLY_CHART`

### `#bintime STARTTIME ENDTIME`

Sets the timerange to import when using the `#incbin` command. 

Both STARTTIME or ENDTIME need to be specified as time in milliseconds, or -1 for Infinity. 

Example:

```
-- Only import Verse 1
#bintime 12000 27000
#incbin my_chart.dsc

-- Import the ending segment from 2:07~
#bintime 127000 -1
#incbin my_chart.dsc
```

### `#const name=arbitrary line of definition`

Defines a macro which will later be replaced in subsequently processed lines. 

Definition doesn't have to be a command or anything, it will be substituted in a simple find-replace fashion.

Example: 

```
-- Camera point definitions for my scene
#const CAMERA_STARTING_POINT=1500, 1500, 17500
#const CAMERA_ENDING_POINT=7500, 1500, 17500
#const CAMERA_LOOKAT_POINT=500, 500, 500
#const NULL_POINT=0, 1000, 0

-- At start of Verse 2
@ 15000
-- Do some camera motions
	MOVE_CAMERA 1500, CAMERA_STARTING_POINT, CAMERA_LOOKAT_POINT, NULL_POINT, CAMERA_ENDING_POINT, CAMERA_LOOKAT_POINT, NULL_POINT, -1, -1
```

### `#undef name`

Undefines a macro defined by `#const`.


### `#sort!`

Initiates a forced timeblock sort of the current state of the script to fix the timing order. 

A timeblock sort is essentially breaking up the whole program into timestamped blocks defined by `TIME` opcodes, removing the actual `TIME` operators, then sorting them ascending, and restoring the `TIME` operators.

This is used i.e. when linking a binary to a script.

Example:

```
-- Include my chart data into my movie
#binflt true
#incbin chart/exported.dsc
#sort!
```

### `#ctxstart` and `#ctxend`

The former creates a context by taking a snapshot of the currently compiled script state.

The latter restores it back. 

Contexts can be nested, e.g. you can create one inside another and they will "stack".

This can be useful for linking with multiple binaries, e.g.:

```
-- CHART EXPORT
#binflt true
#ctxstart // Easy
#incbin chart/Easy.dsc
#sort!
#write build/Easy.dsc
#ctxend

#ctxstart // Normal
#incbin chart/Normal.dsc
#sort!
#write build/Normal.dsc
#ctxend
```

### `#write`

Exports the current state of the script into a dsc binary file.

Usually you want this at the end of the source (unless you need a halfway done binary for some reason).

Example: given above.

### `#unko`

Force stop a compiler and report an error.

Might be useful to mark an incomplete spot in the script to prevent someone else from building it.

### `#for`/`#endfor`

Generate a block of code instead of having to tabulate it externally and hardcode a bunch of lines.

Takes arguments in tuples of `VarName, StartValue, EndValue, Step`.

Example:

```
#for IP_FadeTime 15000 16000 10 IP_FadeValue 1000 0 -10
	@ IP_FadeTime
		SATURATE IP_FadeValue
#endfor
// creates 100 pairs of TIME+SATURATE commands
// with time increasing from 15000 to 16000 in steps of 10,
// saturation decreasing from 1000 to 0 in steps of -10
```

### `#clear`

Discards all commands received before. Usage varies but not too useful outside of REPL mode.

Example:

```
TIME 10
LYRIC 1
#clear
TIME 20
LYRIC 2
#write file.dsc
// file.dsc will contain only: TIME 20, LYRIC 2
```

## Example use-cases

Of course, the primary use of MikuASM is to create your own 3D PV movies. Currently, there is no support for A3DA motions and such, but probably it may arrive in a later version.

One of the biggest problems with writing or editing scripts directly is that note charts consist of an enormous amount of commands, making it easy to mess something up or erase by accident. In addition to that, a variety of GUI chart editors exist, which make charting easier than writing it all by hand in a script. MikuASM leverages the problem by allowing to "overlay" your movie or changes on top of the chart without having to edit it manually.

Assume you have created a chart in UPDC and want to add lyrics to it. Using a script editor, it means you need to carefully add your lyric commands, obeying the timing and avoiding deleting or overwriting something. With MikuASM, it's simpler than ever. 

Create two text files in the same folder as your exported chart file `MyChart.DSC`: `main.mia` and `lyrics.mia`. Inside `lyrics.mia` define your lyric timings according to the following example (`// comments` are optional, but recommended for readability):

```
@ 00:10.0
	LYRIC 1 // "never gonna give you up"
@ 00:16.9
	LYRIC 2 // "never gonna let you down"
@ 00:20.420
	LYRIC 3 // "never gonna run around and desert you"
```

Inside `main.mia` then write the following program:

```
@ 0 // ALWAYS specify the binary input time to be 0
	#incbin MyChart.DSC

// Add lyrics
#include lyrics.mia

// Save file
#sort!
#write MyChart_WithLyrics.DSC
```

Open a command prompt in the folder and execute `mikuasm.exe -v main.mia` or use the MeekStudio IDE to build `main.mia`. A new file appears named `MyChart_WithLyrics.DSC` which contains both your chart and lyric timings.

But what if you now want to add the lyrics to multiple difficulties? No big deal! Change your `main.mia` to the something similar:

```
// Load lyrics
#include lyrics.mia

---- Exporting Easy ----
#ctxstart
	@ 0
	#incbin MyChart_easy.dsc
	#sort!
	#write MyChart_easy_lyrics.dsc
#ctxend

---- Exporting Normal ----
#ctxstart
	@ 0
	#incbin MyChart_norm.dsc
	#sort!
	#write MyChart_norm_lyrics.dsc
#ctxend
```

Once you assemble that, it will generate lyric-enabled versions of both Easy and Normal versions of your chart.

And it doesn't end with just lyrics! (though once there is a GUI editor that supports lyrics, this exact use-case might become negligible)

You can use any commands as in a usual DSC, and overlay them on top of a chart. 

# MeekStudio IDE

MeekStudio is an integrated development environment aimed at creating your own 3D PV movies. Sure, it might be harder to do than using an Edit Mode in one of the games, but the realtime editing features allow for fast learning of commands and higher turnaround time between iterations of scenes, making it an extremely handy tool for those familiar with assembly-like programming languages.

## Capabilities of MeekStudio

* Edit MikuASM source code with syntax highlighting and quick hints.
* Edit PV DB text files and sort them in the proper field order. When lines are moved during sorting, comments are retained, too!
* View decompiled sources of DSC files for introspection of your build artifacts and chart data.
* Attach to the game as a debug tool to quickly move camera and characters, adjust scenes and visually see what commands are doing.
* Use the Build & Play feature to watch a full version of your movie without even touching the game files and/or extra data.

## Using MeekStudio to edit projects

You can create a MeekStudio project using the toolbar icon or the Project → Create... menu entry. The last active project will be loaded every time you start MeekStudio.

A project is basically a folder which can contain other folders and files. It also has a special `meta.msproj` file to tell MeekStudio that this folder is a project and what are the key files inside it.

In order to use various debugging features and Build & Watch, you need to tell MeekStudio, which of the files have which role. To do this, right-click on the file in the Project Hierarchy view, and select Set As → PV DB file, Music file, Movie DSC file or Entrypoint file.

A PV DB file contains the PV metadata for your movie, such as what audio to use or what is it's title. 

A Music file is an OGG file that is used as the background audio for your movie.

A Movie DSC file is the output DSC of your project that is used when previewing with Build & Watch. It is recommended to build it without the chart inside a separate context (see `#ctxstart`/`#ctxend`).

An entrypoint file is the file where your project build will start from, usually `main.mia`. 

To view a text file, DSC file or MIA file, double-click on it in the Project Hierarchy view. Double-clicking other file types does the same thing as double-clicking them in Windows Explorer.

To close an editor tab, click on the tab header's right side or click on the tab header with the mouse wheel button. The editor content will be saved upon closing it.

## Using MeekStudio's assistance features

Without the assistance features, MeekStudio is pretty much an example of an awful text editor. However using the assistance features you can edit and arrange your scenes much faster.

Because attaching to a running game process is usually not very stable, it's recommended to use the "Boot game to debug" function. In order to use that, you will need to use the Debugger → Set game exe path... menu item to select a valid vanilla game executable of version exactly 7.10. Obtaining such executables in a legal and clean way is your own responsibility here.

### Boot game to debug

This will launch the game in a sped-up startup sequence and start up an empty scene with your PV DB metadata.

Once that is done, you can use the assistance features in full. Please note that changing your PV DB metadata requires you to exit the game and use "Boot game to debug" again to reload it.

### Execute while editing

When enabled, any command line that you input in the script editor, once recognized as a valid command, will be executed by the game's engine. 

This is pretty useful when editing things that can only be seen experimentally like `SATURATE` or `TONE_TRANS` — you just edit your code and see the changes in realtime.

However, it might cause problems if you run a bad command, so it's recommended to keep this option off when not using it.

### Evaluate current line (F9)

Tells the game engine to execute the line where your cursor is currently at. Please note it will ignore certain operators such as `END`, `PV_END` or `TIME`.

### Evaluate until current line (Shift+F9)

Tells the game engine to execute all the operator lines above and including the one your cursor is currently at. `END`, `PV_END` and `TIME` are ignored.

### Evaluate current file (Ctrl+F9)

Tells the game engine to execute all the operator lines within the currently edited file. `END`, `PV_END` and `TIME` are ignored.

### Interactive → Camera move wizard (F10)

Brings up the Camera Mover dialog box. If your cursor is currently at a `MOVE_CAMERA` operator line, loads the parameters from there into the Camera Mover and sets the game camera to show the starting position. Otherwise, retrieves the current camera position from the game.

Within the Camera Mover, you can scroll with the mouse wheel while hovering a text field to change and preview the coordinates in a speedy coarse fashion. You can also input a duration and click Animate to preview the path between the two points you have selected. Please note the animation does not account for ease-in, ease-out parameters.

You can use the Fetch button to recall a coordinate from what the game engine window is currently displaying, or Preview button to tell the game engine to display the entered coordinate. 

Once you click the Insert into script button, a `MOVE_CAMERA` operator will be formed and inserted at the current cursor position in the active editor tab.

### Interactive → Chara move wizard (F11)

Brings up the Chara Mover dialog box. If your cursor is currently at a `MIKU_MOVE` or `MIKU_ROT` operator line, loads the parameters from it into the respective part of the Chara Mover dialog. Otherwise, retrieves the current character position from the game. 

Within the Chara Mover, you can scroll with the mouse wheel while hovering a text field to change and preview the coordinates in a speedy coarse fashion.

Once you click the Insert into script button, a `MIKU_MOVE` or `MIKU_ROT` operator will be formed and inserted at the current cursor position in the active editor tab.

### Interactive → Boot and play movie (Ctrl+F5)

This will launch the game in a sped-up startup sequence and start up your PV in whole. Your PV DB metadata will be used as PV DB, your Movie DSC will be used as the script, and your Music file will be used for the music audio.

Use this function to preview your movie in whole. To continue working with the editor, finish the preview by closing the game engine window.

# MikuASM.exe

MikuASM.exe is an assembler/linker for DSC game engine script binaries. It can be integrated into your build system of choice using a Makefile or any other method of running commands.

Pretty much it is a barebones implementation of the MikuASM language interpreter. 

## Capabilities of MikuASM.exe

* Assemble DSC binaries from assembly-like source code
* Include pre-assembled binaries into source code
* Correct timing when linking multiple scripts together
* Assemble multiple binaries from same source by linking different binaries in
* Interactive console (mostly useless but can display command reference) by `--interactive`
* Direct engine control for seeing what commands do with `--attach`
* Export command reference text file by `--dict`

## Interactive console commands

You can use the following extra commands in the interactive console mode:

* `#exit` -- exit the interpreter
* `?COMMAND` -- get the information about `COMMAND`s arguments and usage description

## Interactive console debugger

If you specify `--attach` on the command line with `--interactive`, MikuASM will try to connect to a game executable when launching the interactive console.

Once it's connected, all commands input will be executed directly inside the game engine. You can use this to see the effect of the commands on the state of the scene.

Then you can enter the game and start a PV of interest (in order to use it's PVDB data). The PV won't play, and the engine will respond to your commands from the console.

Please note that TIME commands will not work from the console, and issuing an END or PV_END command will end the engine session. Once the session is ended, you need to restart the game and the console.

