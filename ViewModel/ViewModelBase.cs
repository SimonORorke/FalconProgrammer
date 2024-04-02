﻿using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ViewModelBase(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService) : ObservableRecipient {
  private IFileSystemService? _fileSystemService;
  private ModelServices? _modelServices;
  private ISerialiser? _serialiser;
  private Settings? _settings;
  private SettingsReader? _settingsReader;
  protected IDialogWrapper DialogWrapper { get; } = dialogWrapper;
  protected IDispatcherService DispatcherService { get; } = dispatcherService;

  internal IFileSystemService FileSystemService =>
    _fileSystemService ??= ModelServices.GetService<IFileSystemService>();

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

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  protected ISerialiser Serialiser =>
    _serialiser ??= ModelServices.GetService<ISerialiser>();

  internal ModelServices ModelServices {
    [ExcludeFromCodeCoverage] get => _modelServices ??= ModelServices.Default;
    // For unit testing.
    set => _modelServices = value;
  }

  internal Settings Settings {
    get => _settings ??= ReadSettings();
    private set => _settings = value;
  }

  private SettingsReader SettingsReader =>
    _settingsReader ??= ModelServices.GetService<SettingsReader>();

  protected void GoToLocationsPage() {
    // using CommunityToolkit.Mvvm.Messaging is needed to provide this Send extension
    // method.
    Messenger.Send(new GoToLocationsPageMessage());
  }

  public virtual void Open() {
    // Debug.WriteLine($"ViewModelBase.Open: {GetType().Name}");
    IsVisible = true;
    IsActive = true; // Start listening for ObservableRecipient messages.
    Settings = ReadSettings();
  }

  public virtual bool QueryClose() {
    // Debug.WriteLine($"ViewModelBase.QueryClose: {GetType().Name}");
    IsVisible = false;
    IsActive = false; // Stop listening for ObservableRecipient messages.
    return true;
  }

  private Settings ReadSettings() {
    // Debug.WriteLine($"ViewModelBase.ReadSettings: {GetType().Name}");
    return SettingsReader.Read(true);
  }
}