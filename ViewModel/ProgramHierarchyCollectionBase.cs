using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class ProgramHierarchyCollectionBase<T> : DataGridItemCollectionBase<T>
  where T : SoundBankItem {
  protected ProgramHierarchyCollectionBase(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(dispatcherService) {
    FileSystemService = fileSystemService;
  }

  protected IFileSystemService FileSystemService { get; }
  protected ImmutableList<string> SoundBanks { get; set; } = [];
  internal abstract void Populate(Settings settings, IEnumerable<string> soundBanks);
  internal abstract void UpdateSettings();
}