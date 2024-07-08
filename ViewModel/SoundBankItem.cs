using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankItem : DataGridItemBase {
  internal const string AllCaption = "All";

  /// <summary>
  ///   Generates <see cref="SoundBank" /> property.
  /// </summary>
  [ObservableProperty] private string _soundBank = string.Empty;

  public SoundBankItem(
    Settings settings,
    IFileSystemService fileSystemService,
    bool isAdditionItem) : base(isAdditionItem) {
    FileSystemService = fileSystemService;
    Settings = settings;
  }

  public ImmutableList<string> SoundBanks { get; internal set; } = [];
  protected IFileSystemService FileSystemService { get; }
  protected Settings Settings { get; }

  // Unit test code coverage highlighting for partial methods that are based on generated
  // code requires this setting change in Jetbrains Rider: 
  // File | Settings | Build, Execution, Deployment | dotCover | Filtering |
  // Do not analyze code marked with attributes |
  // Disable System.CodeDom.Compiler.GeneratedCodeAttribute 
  partial void OnSoundBankChanged(string value) {
    OnSoundBankChanged1(value);
  }

  /// <summary>
  ///   Because the generated OnSoundBankChanged method is private
  /// </summary>
  protected virtual void OnSoundBankChanged1(string value) { }
}