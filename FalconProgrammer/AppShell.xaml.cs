using FalconProgrammer.ViewModel;

namespace FalconProgrammer;

public partial class AppShell : Shell {
  private AppShellViewModel? _viewModel;
  
  public AppShell() {
    InitializeComponent();
  }

  private AppShellViewModel ViewModel => 
    _viewModel ??= (AppShellViewModel)BindingContext;

  protected override void OnNavigated(ShellNavigatedEventArgs args) {
    base.OnNavigated(args);
    ViewModel.OnNavigated();
  }

  // Something like this commented out code could be useful if we persist window size and
  // position.
  // protected override void OnAppearing() {
  //   base.OnAppearing();
  //   const int windowWidth = 1155;
  //   if (DeviceInfo.Current.Platform == DevicePlatform.WinUI) {
  //     Window!.Width = windowWidth;
  //   } else if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst) {
  //     // According to 
  //     // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows?#position-and-size-a-window,
  //     // Mac Catalyst doesn't support resizing or repositioning windows programmatically
  //     // by setting the X, Y, Width, and Height properties.
  //     // However, this workaround is provided.
  //     // I've tried it with Windows. It works. However, for Windows, the simple way is
  //     // probably more efficient, so we are sticking with that.
  //     Window!.MinimumWidth = windowWidth;
  //     Window.MaximumWidth = windowWidth;
  //     // Give the Window time to resize
  //     Dispatcher.Dispatch(() => {
  //       // I don't quite understand why this works. The brief explanation in the
  //       // Microsoft documentation is not illuminating. I think the following minima and
  //       // maxima must be invalid, somehow forcing the width within the bounds of the
  //       // previously defined identical minimum and maximum.
  //       Window.MinimumWidth = 0;
  //       Window.MinimumHeight = 0;
  //       Window.MaximumWidth = double.PositiveInfinity;
  //       Window.MaximumHeight = double.PositiveInfinity;
  //     });
  //   }
  // }
}