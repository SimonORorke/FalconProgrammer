namespace FalconProgrammer.Views;

public partial class GuiScriptProcessorPage : ContentPageBase {

  public GuiScriptProcessorPage() : base(ContentPageTitle) {
    InitializeComponent();
  }

  private static string ContentPageTitle => 
    "Falcon program categories that must use a GUI script processor";
  public static string TabTitle => "GUI Script Processor";
}