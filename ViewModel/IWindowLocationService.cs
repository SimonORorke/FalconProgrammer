namespace FalconProgrammer.ViewModel;

/// <summary>
///   Interface for a service that facilitates persistence of the main window's position,
///   size and state.
/// </summary>
public interface IWindowLocationService {
  int? Left { get; set; }
  int? Top { get; set; }
  int? Width { get; set; }
  int? Height { get; set; }
  int? WindowState { get; set; }

  /// <summary>
  ///   Uses previously saved data, if available, to size the window and position it on a
  ///   screen.
  /// </summary>
  void Restore();

  /// <summary>
  ///   Updates the data to be saved to settings from the window's current
  ///   position/size/state.
  /// </summary>
  void Update();
}