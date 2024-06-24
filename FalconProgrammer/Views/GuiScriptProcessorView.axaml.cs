﻿using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class GuiScriptProcessorView : UserControl {
  public GuiScriptProcessorView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new GuiScriptProcessorViewModel(new DialogService(), new DispatcherService()));
    }
    InitializeComponent();
  }
}