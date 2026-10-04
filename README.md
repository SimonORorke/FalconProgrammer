# Falcon Programmer

Falcon Programmer is an open source batch configuration application for the [UVI Falcon](https://www.uvi.net/falcon.html/) software synthesizer. Multiple types of configuration change can be implemented in thousands of Falcon programs with a single batch run, taking seconds to minutes.

There are installers for Windows (64-bit) and macOS (Universal: Apple Silicon + Intel).

## The following configuration tasks are available:

 ·    Restore the program to an original version, ready for the configuration changes to be made.

·    Initialise the program's Info page layout with many options, including converting a script-based layout to the standard layout.

·    Assign MIDI CC numbers to macros and, provided the program uses the standard Info page layout, optionally append each macro's MIDI CC number to its display name.

·    Remove arpeggiators and sequencing script processors and then, provided the program has a standard Info page, remove any macros that consequently no longer modulate anything.

Bypass (disable) all known delay effects and then, provided the program uses the standard Info page layout, remove any macro that no longer modulates any enabled effects.

·    If a Release macro is not part of a set of four ADSR macros and the macro is not modulated by the mod wheel, set its initial value to zero.

·    Set the values of known reverb macros, with some exceptions, to zero.

·    Move release and reverb macros that have zero values to the end of the standard Info page layout.

·    Where feasible, replace all modulations by the modulation wheel with modulations by a new 'Wheel' macro on the standard Info page layout.

·    If the modulation wheel's modulations have been reassigned to a Wheel macro (the previous task), reuse MIDI CC 1 (the mod wheel) for a subsequent macro, where feasible.

·    Prepend a line indicating the program's path (sound bank\category\program name) to the program's description, which is viewable in Falcon when the Info page's ***i*** button is clicked.

Of these configuration tasks, assigning MIDI CC numbers to macros will be of use to many Falcon players. And restoring the program to an original version is just a safety feature to facilitate subsequent transformation. The remainder are merely what the developer has found useful as a Falcon player. Many more configuration tasks are surely possible. Users of the application are welcome to suggest some!

## macOS First-Time Launch Instructions

In the DMG, drag **Falcon Programmer** into /Applications. Then take one of the following approaches.

### Approach A

1. Open the app. When a security prompt headed **''“Falcon Programmer” Not Opened'**  appears, click **Done**. *Do not click Move to Bin*.
2. Go to **System Settings** → **Privacy & Security**, scroll down to **Security**, and click **Open Anyway**.
3. On the confirmation prompt, click **Open Anyway**. *(This is only required on the very first launch).*

### Approach B

Run this in a terminal:

```bash
xattr -cr "/Applications/Falcon Programmer Bridge.app"
```

Once executed, the application will open directly upon double-clicking.

## Further information

For comprehensive documentation, please refer to [the manual](Documentation/Falcon%20Programmer%20Manual.pdf).

Simon O'Rorke

