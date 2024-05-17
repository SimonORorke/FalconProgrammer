namespace FalconProgrammer.ViewModel;

/// <summary>
///   View model for a dialog window that shows large messages with scrollbars.
/// </summary>
public class MessageWindowViewModel {
  private IApplicationInfo? _applicationInfo;
  
  /// <summary>
  ///   Initialises a new instance of the <see cref="MessageWindowViewModel" /> class.
  /// </summary>
  /// <param name="text">The message body text.</param>
  /// <param name="title">
  ///   The window title, excluding the '[application name] - ' prefix, which will be
  ///   added.
  /// </param>
  public MessageWindowViewModel(string text, string title) {
    Text = text;
    Title = $"{ApplicationInfo.Product} - {title}";
  }
  
  internal IApplicationInfo ApplicationInfo {
    get => _applicationInfo ??= new ApplicationInfo();
    set => _applicationInfo = value;
  }

  public string Text { get; }
  public string Title { get; }
}