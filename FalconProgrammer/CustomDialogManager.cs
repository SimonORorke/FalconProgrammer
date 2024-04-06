using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Logging;

namespace FalconProgrammer;

/// <summary>
///   DialogManager for Avalonia, customised for view models that are in a different
///   assembly from views.
/// </summary>
public class CustomDialogManager(
  IViewLocator? viewLocator = null,
  IDialogFactory? dialogFactory = null,
  ILogger<DialogManager>? logger = null,
  IDispatcher? dispatcher = null,
  Control? customNavigationRoot = null)
  : DialogManager(viewLocator, dialogFactory, logger, dispatcher, customNavigationRoot) {
  /// <inheritdoc />
  public override IView? FindViewByViewModel(INotifyPropertyChanged viewModel) {
    var viewLocator = (ViewLocator)ViewLocator;
    return viewLocator.ContentControlViewInstance != null
      // E.g. view is a UserControl.
      ? AsWrapper(viewLocator.ContentControlViewInstance)
      // Maybe view is a Window. But this method does not get executed for at least the
      // main window view model, which is currently the only window view model.
      : base.FindViewByViewModel(viewModel);
  }
}