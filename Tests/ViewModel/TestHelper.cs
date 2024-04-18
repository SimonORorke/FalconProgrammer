using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

internal static class TestHelper {
  public static CcNoRangeViewModel CreateCcNoRangeAdditionItem(int? start, int? end) {
    return new CcNoRangeViewModel(
      AppendAdditionItem, OnItemChanged, RemoveItem, true,
      CutItem, PasteBeforeItem) {
      Start = start,
      End = end
    };
  }

  [ExcludeFromCodeCoverage]
  private static void AppendAdditionItem() { }

  [ExcludeFromCodeCoverage]
  private static void CutItem(DataGridItem itemToCut) { }

  [ExcludeFromCodeCoverage]
  private static void OnItemChanged() { }

  [ExcludeFromCodeCoverage]
  private static void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) { }

  [ExcludeFromCodeCoverage]
  private static void RemoveItem(DataGridItem itemToRemove) { }
}