using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FalconProgrammer.WinUI;

/// <summary>
///   Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication {
  /// <summary>
  ///   Initializes the singleton application object.  This is the first line of authored code
  ///   executed, and as such is the logical equivalent of main() or WinMain().
  /// </summary>
  public App() {
    InitializeComponent();
    // For Windows, there is a known issue where a CollectionView's Header and Footer 
    // are not shown. See https://github.com/dotnet/maui/issues/14557, 
    // where this workaround is provided.
    // The issue is supposed to have been fixed.
    // See https://github.com/dotnet/maui/pull/16870. But I still need this workaround.
    // So I've raised a new issue: https://github.com/dotnet/maui/issues/21224.
    CollectionViewHandler.Mapper.AppendToMapping("HeaderAndFooterFix",
      (_, collectionView) => {
        collectionView.AddLogicalChild(collectionView.Header as Element);
        collectionView.AddLogicalChild(collectionView.Footer as Element);
      });
  }

  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}