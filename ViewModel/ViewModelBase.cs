using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService) : ObservableRecipient {
  protected IDialogWrapper DialogWrapper { get; } = dialogWrapper;
  protected IDispatcherService DispatcherService { get; } = dispatcherService;

  protected bool IsVisible { get; private set; }

  /// <summary>
  ///   Title to be shown at the top of the main window when the page is selected and
  ///   shown.
  /// </summary>
  public abstract string PageTitle { get; }

  /// <summary>
  ///   Title to be shown on the page's tab. Defaults to the same as
  ///   <see cref="PageTitle" />.
  /// </summary>
  public virtual string TabTitle => PageTitle;

  protected void GoToLocationsPage() {
    // using CommunityToolkit.Mvvm.Messaging is needed to provide this Send extension
    // method.
    Messenger.Send(new GoToLocationsPageMessage());
  }

  public virtual void Open() {
    // Debug.WriteLine($"ViewModelBase.Open: {GetType().Name}");
    IsVisible = true;
    IsActive = true; // Start listening for ObservableRecipient messages.
  }

  public virtual bool QueryClose() {
    // Debug.WriteLine($"ViewModelBase.QueryClose: {GetType().Name}");
    IsVisible = false;
    IsActive = false; // Stop listening for ObservableRecipient messages.
    return true;
  }
}