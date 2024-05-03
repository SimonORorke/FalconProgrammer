using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankItem : DataGridItem {
  internal const string AllCaption = "All";

  [ObservableProperty]
  private string _soundBank = string.Empty; // Generates SoundBank property

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

  // Code coverage highlighting does not work for these partial methods.
  partial void OnSoundBankChanged(string value) {
    OnSoundBankChanged1(value);
  }

  /// <summary>
  ///   Because the generated OnSoundBankChanged method is private
  /// </summary>
  protected virtual void OnSoundBankChanged1(string value) { }
}