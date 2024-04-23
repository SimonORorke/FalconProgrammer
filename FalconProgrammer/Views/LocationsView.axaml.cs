using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class LocationsView : UserControl {
  public LocationsView() {
    InitializeComponent();
    // This only sets the DataContext for the previewer in the IDE.
    Design.SetDataContext(this,
      new LocationsViewModel(new DialogService(), new DispatcherService()));
    // SettingsFolderButton.GotFocus += SettingsFolderButtonOnGotFocus;
  }

  // This proves that when the Settings Folder label's alt-key shortcut is pressed,
  // the Browse button does get focus. The problem is that there's no focus rectangle.
  // TODO: Investigate lack of focus rectangle on button when label's alt access key is pressed.
  // private void SettingsFolderButtonOnGotFocus(object? sender, GotFocusEventArgs e) {
  //   Debug.WriteLine("LocationsView.SettingsFolderButtonOnGotFocus");
  // }
}