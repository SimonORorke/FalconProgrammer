namespace FalconProgrammer.Model;

public enum ConfigTask {
  CountMacros, // TODO: Rename CountMacros to QueryCountMacros 
  InitialiseLayout,
  InitialiseValuesAndMoveMacros,
  MoveConnectionsBeforeProperties,
  PrependPathLineToDescription,
  QueryAdsrMacros,
  QueryDahdsrModulations,
  QueryDelayTypes,
  QueryMainDahdsr,
  QueryReverbTypes,
  QueryReuseCc1NotSupported,
  RemoveDelayEffectsAndMacros,
  ReplaceModWheelWithMacro,
  RestoreOriginal,
  ReuseCc1,
  UpdateMacroCcs
}