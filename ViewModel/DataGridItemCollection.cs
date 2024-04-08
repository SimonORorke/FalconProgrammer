﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class DataGridItemCollection<T>(
  Settings settings,
  IDispatcherService dispatcherService) : ObservableCollection<T> where T : DataGridItem {
  private bool _isPopulating;
  private IDispatcherService DispatcherService { get; } = dispatcherService;
  private bool ForceAppendAdditionItem { get; set; }

  /// <summary>
  ///   Gets whether the collection has changed being populated.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }
  protected bool IsAddingAdditionItem => !IsPopulating || ForceAppendAdditionItem;

  protected bool IsPopulating {
    get => _isPopulating;
    set {
      if (value) {
        ForceAppendAdditionItem = false;
      } else {
        ForceAppendAdditionItem = true;
        AppendAdditionItem(); 
        ForceAppendAdditionItem = false;
        HasBeenChanged = false;
      }
      _isPopulating = value;
    }
  }

  protected Settings Settings { get; } = settings;

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  protected abstract void AppendAdditionItem();
  
  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    base.OnCollectionChanged(e);
    if (IsPopulating) {
      return;
    }
    HasBeenChanged = true;
  }

  protected void OnItemChanged() {
    HasBeenChanged = true;
  }

  // This looks like a false suggestion we are having to suppress.
  // AppendAdditionItem does not get the suggestion. Why does this method?
  // ReSharper disable once UnusedMemberInSuper.Global
  protected abstract void RemoveItem(ObservableObject itemToRemove);

  protected void RemoveItemTyped(T itemToRemove) {
    DispatcherService.Dispatch(() => Remove(itemToRemove));
  }
}