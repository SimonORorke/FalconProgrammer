# Falcon Programmer Release Notes

## Version 1.1

### Enhancement

New configuration task **RemoveArpeggiatorsAndSequencing**:  Removes arpeggiators and sequencing script processors. Then, provided the program has a standard Info page, removes any macros that consequently no longer modulate anything.

### Bug Fix

Fixed a bug where the **RemoveDelayEffectsAndMacros** task removed toggle macros that were not delay macros but were initially toggled off.

## Version 1.0.1

### Enhancement

**MIDI for Macros page**: Added advice 'MIDI CC 38 does not work when assigned to a control on a script-based Info page'.

### Bug Fix

Fixed a bug where, when the **InitialiseLayout** task removed a GUI script processor, all other program-level event processors were removed too.

