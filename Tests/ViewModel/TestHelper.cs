using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public static class TestHelper {

  public static CcNoRangeViewModel CreateCcNoRangeAdditionItem(int start, int end) {
    return new CcNoRangeViewModel(
      AppendAdditionItem, OnItemChanged, RemoveItem, true) {
      Start = start,
      End = end
    };
  }

  [ExcludeFromCodeCoverage]
  private static void AppendAdditionItem() { }

  [ExcludeFromCodeCoverage]
  private static void OnItemChanged() { }

  [ExcludeFromCodeCoverage]
  private static void RemoveItem(ObservableObject itemToRemove) { }
}