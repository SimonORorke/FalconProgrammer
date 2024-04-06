using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public abstract partial class SettingsWriterViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService) {
  [ObservableProperty] private string _settingsFolderPath = string.Empty;
}