using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class BackgroundView : UserControl {
  public BackgroundView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new BackgroundViewModel(new DialogService(), new DispatcherService()));
    }
    InitializeComponent();
    DataGrid.PreparingCellForEdit += DataGridHelper.OnPreparingCellForEdit;
  }

  /// <summary>
  ///   A Browse Button in the DataGrid was clicked, or a click was emulated with the F2
  ///   keyboard shortcut (see <see cref="DataGridHelper.OnPreparingCellForEdit" />.
  ///   Because the Browse dialog was then shown and closed asynchronously,
  ///   focus has been lost and returned to the Window. So focus the DataGrid,
  ///   which will have retained its current cell.
  /// </summary>
  /// <remarks>
  ///   If the current cell is the Browse cell, which may not be the case if the button
  ///   was clicked with the mouse, it will not get its focus rectangle back.
  ///   But that is consistent with what happens if the Edit button is clicked or has
  ///   an emulated click.
  /// </remarks>
  private void BackgroundsOnBrowsed(object? sender, EventArgs e) {
    DataGrid.Focus();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var backgrounds = ((BackgroundViewModel)DataContext!).Backgrounds;
    backgrounds.Browsed += BackgroundsOnBrowsed;
  }
}