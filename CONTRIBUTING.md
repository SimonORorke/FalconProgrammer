# Contributing to Falcon Programmer development

Contributions to the range of batch configuration tasks offered by Falcon Programmer are welcome.

### Architecture

The application is written in C#/.NET.  The GUI is built with the open source cross-platform [Avalonia UI](https://avaloniaui.net/). (If you are looking at the Avalonia UI home page for the first time, ignore the possibly confusing confusing link right at the top to Avalonia XPF, which allows WPF applications to compile and run on non-Windows platforms.  Avalonia XPF is a commercial product and not what Falcon Programmer uses.)  Avalonia UI is WPF-like, but it is not WPF.  Falcon Programmer stores data to text files in XML format.

With this architecture, Falcon Programmer should run on macOS, even though it has so far been developed only on Windows.  As UVI Falcon runs on both Windows and macOS, making Falcon Programmer available for macOS is highly desirable.

### Creating a macOS installer

So I especially need a collaborator to give the application some basic testing on a Mac create and create the macOS installer.  *For this particular role*, you don't necessarily need experience of the C# programming language. But you will need to compile the program, run the unit tests and follow the [macOS-specific instructions](https://docs.avaloniaui.net/docs/deployment/macOS) to create an application bundle that will deploy Avalonia UI.  And presumably you have a copy of UVI Falcon on your Mac, or you have been asked to do this task by someone who does!  I only have Windows computers and experience, but I will help where I can.

### Development Tools

If you are new to .NET development, you will need an integrated development environment (IDE).  I can highly recommend what I use, [Jetbrains Rider](https://www.jetbrains.com/rider/) (Windows and macOS; commercial, but free for students and teachers).  Otherwise there is Microsoft's [Visual Studio](https://visualstudio.microsoft.com/) (Windows; commercial).  Microsoft are about to [discontinue support for Visual Studio for Mac](https://learn.microsoft.com/en-us/visualstudio/mac/what-happened-to-vs-for-mac).  [Visual Studio Community Edition](https://visualstudio.microsoft.com/vs/community/) (Windows only) is free for personal and open source use and has all the Visual Studio features that will be useful for developing Falcon Programmer.  [Visual Studio Code](https://code.visualstudio.com/) (Windows and macOS; free) is described as a "code editor" rather than an IDE, meaning it is feature-poor compared with any of the other options.

All of the above support editing XML files.  Other XML editor options are discussed in [the Falcon Programmer manual](Documentation/Falcon Programmer Manual v1.0.pdf).

Simon O'Rorke

