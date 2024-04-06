﻿using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory(
  // 'partial' allows CommunityToolkit.Mvvm code generation based on ObservableProperty
  // and RelayCommand attributes.
  Action appendAdditionItem,
  Action onItemChanged,
  Action<SoundBankCategory> removeItem)
  : ObservableObject {
  public const string AllCategoriesCaption = "All";
  [ObservableProperty] private bool _canRemove; // Generates CanRemove property

  [ObservableProperty]
  private string _category = string.Empty; // Generates Category property

  [ObservableProperty]
  private string _soundBank = string.Empty; // Generates SoundBank property

  public ImmutableList<string> SoundBanks { get; internal set; } = [];
  private bool IsAdding { get; set; }
  internal bool IsAdditionItem => SoundBank == string.Empty;
  internal bool IsForAllCategories => Category == AllCategoriesCaption;
  public ObservableCollection<string> Categories { get; } = [];
  private Action AppendAdditionItem { get; } = appendAdditionItem;
  private Action OnItemChanged { get; } = onItemChanged;
  private Action<SoundBankCategory> RemoveItem { get; } = removeItem;

  // Code coverage highlighting does not work for these partial methods.
  partial void OnSoundBankChanged(string value) {
    // On addition after removal, the new sound bank is null.
    // This fixes it.
    if (string.IsNullOrWhiteSpace(value)) {
      return;
    }
    PopulateCategories();
    CanRemove = true;
    Category = AllCategoriesCaption;
    if (IsAdding) {
      // The user has used up the addition item, the one at the end with the blank
      // sound bank and category. So we need to append another addition item to the
      // collection.
      AppendAdditionItem();
      IsAdding = false;
    }
  }

  // ReSharper disable once UnusedParameterInPartialMethod
  partial void OnSoundBankChanging(string value) {
    IsAdding = IsAdditionItem;
  }

  private void PopulateCategories() {
    Categories.Clear();
    Categories.Add(AllCategoriesCaption);
    string[] categoryFolderNames = ["Bass", "Lead", "Pad", "Pluck"];
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (e.PropertyName is nameof(SoundBank)
        or nameof(Category)) {
      OnItemChanged();
    }
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  [RelayCommand] // Generates RemoveCommand
  private void Remove() {
    RemoveItem(this);
  }
}