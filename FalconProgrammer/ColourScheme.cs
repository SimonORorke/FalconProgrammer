using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer;

public static class ColourScheme {
  public static void Select(ColourSchemeId scheme) {
    var newFluentTheme = new FluentTheme();
    newFluentTheme.Palettes.Clear();
    var lightKey = new ThemeVariant("Light", null);
    var darkKey = new ThemeVariant("Dark", null);
    var light = new ColorPaletteResources();
    var dark = new ColorPaletteResources();
    newFluentTheme.Palettes.Add(lightKey, light);
    newFluentTheme.Palettes.Add(darkKey, dark);
    switch (scheme) {
      case ColourSchemeId.Default:
        SelectDefaultColourScheme(light, dark);
        break;
      case ColourSchemeId.Forest:
        SelectForestColourScheme(light, dark);
        break;
      case ColourSchemeId.Lavender:
        SelectLavenderColourScheme(light, dark);
        break;
      case ColourSchemeId.Nighttime:
        SelectNighttimeColourScheme(light, dark);
        break;
    }
    Application.Current!.Styles[0] = newFluentTheme;
  }

  /// <summary>
  ///   Default palette preset from the online Fluent Editor https://theme.xaml.live/.
  /// </summary>
  private static void SelectDefaultColourScheme(
    ColorPaletteResources light,
    ColorPaletteResources dark) {
    light.Accent = Color.Parse("#ff0073cf");
    light.AltHigh = Color.Parse("White");
    light.AltLow = Color.Parse("White");
    light.AltMedium = Color.Parse("White");
    light.AltMediumHigh = Color.Parse("White");
    light.AltMediumLow = Color.Parse("White");
    light.BaseHigh = Color.Parse("Black");
    light.BaseLow = Color.Parse("#ffcccccc");
    light.BaseMedium = Color.Parse("#ff898989");
    light.BaseMediumHigh = Color.Parse("#ff5d5d5d");
    light.BaseMediumLow = Color.Parse("#ff737373");
    light.ChromeAltLow = Color.Parse("#ff5d5d5d");
    light.ChromeBlackHigh = Color.Parse("Black");
    light.ChromeBlackLow = Color.Parse("#ffcccccc");
    light.ChromeBlackMedium = Color.Parse("#ff5d5d5d");
    light.ChromeBlackMediumLow = Color.Parse("#ff898989");
    light.ChromeDisabledHigh = Color.Parse("#ffcccccc");
    light.ChromeDisabledLow = Color.Parse("#ff898989");
    light.ChromeGray = Color.Parse("#ff737373");
    light.ChromeHigh = Color.Parse("#ffcccccc");
    light.ChromeLow = Color.Parse("#ffececec");
    light.ChromeMedium = Color.Parse("#ffe6e6e6");
    light.ChromeMediumLow = Color.Parse("#ffececec");
    light.ChromeWhite = Color.Parse("White");
    light.ListLow = Color.Parse("#ffe6e6e6");
    light.ListMedium = Color.Parse("#ffcccccc");
    light.RegionColor = Color.Parse("White");
    dark.Accent = Color.Parse("#ff0073cf");
    dark.AltHigh = Color.Parse("Black");
    dark.AltLow = Color.Parse("Black");
    dark.AltMedium = Color.Parse("Black");
    dark.AltMediumHigh = Color.Parse("Black");
    dark.AltMediumLow = Color.Parse("Black");
    dark.BaseHigh = Color.Parse("White");
    dark.BaseLow = Color.Parse("#ff333333");
    dark.BaseMedium = Color.Parse("#ff9a9a9a");
    dark.BaseMediumHigh = Color.Parse("#ffb4b4b4");
    dark.BaseMediumLow = Color.Parse("#ff676767");
    dark.ChromeAltLow = Color.Parse("#ffb4b4b4");
    dark.ChromeBlackHigh = Color.Parse("Black");
    dark.ChromeBlackLow = Color.Parse("#ffb4b4b4");
    dark.ChromeBlackMedium = Color.Parse("Black");
    dark.ChromeBlackMediumLow = Color.Parse("Black");
    dark.ChromeDisabledHigh = Color.Parse("#ff333333");
    dark.ChromeDisabledLow = Color.Parse("#ff9a9a9a");
    dark.ChromeGray = Color.Parse("Gray");
    dark.ChromeHigh = Color.Parse("Gray");
    dark.ChromeLow = Color.Parse("#ff151515");
    dark.ChromeMedium = Color.Parse("#ff1d1d1d");
    dark.ChromeMediumLow = Color.Parse("#ff2c2c2c");
    dark.ChromeWhite = Color.Parse("White");
    dark.ListLow = Color.Parse("#ff1d1d1d");
    dark.ListMedium = Color.Parse("#ff333333");
    dark.RegionColor = Color.Parse("Black");
  }

  /// <summary>
  ///   Forest palette preset from the online Fluent Editor https://theme.xaml.live/.
  /// </summary>
  private static void SelectForestColourScheme(
    ColorPaletteResources light,
    ColorPaletteResources dark) {
    light.Accent = Color.Parse("#ff34854d");
    light.AltHigh = Color.Parse("White");
    light.AltLow = Color.Parse("White");
    light.AltMedium = Color.Parse("White");
    light.AltMediumHigh = Color.Parse("White");
    light.AltMediumLow = Color.Parse("White");
    light.BaseHigh = Color.Parse("Black");
    light.BaseLow = Color.Parse("#ffc2db65");
    light.BaseMedium = Color.Parse("#ff7d9728");
    light.BaseMediumHigh = Color.Parse("#ff4f6a00");
    light.BaseMediumLow = Color.Parse("#ff668114");
    light.ChromeAltLow = Color.Parse("#ff4f6a00");
    light.ChromeBlackHigh = Color.Parse("Black");
    light.ChromeBlackLow = Color.Parse("#ffc2db65");
    light.ChromeBlackMedium = Color.Parse("#ff4f6a00");
    light.ChromeBlackMediumLow = Color.Parse("#ff7d9728");
    light.ChromeDisabledHigh = Color.Parse("#ffc2db65");
    light.ChromeDisabledLow = Color.Parse("#ff7d9728");
    light.ChromeGray = Color.Parse("#ff668114");
    light.ChromeHigh = Color.Parse("#ffc2db65");
    light.ChromeLow = Color.Parse("#ffe6f3bb");
    light.ChromeMedium = Color.Parse("#ffdfeeaa");
    light.ChromeMediumLow = Color.Parse("#ffe6f3bb");
    light.ChromeWhite = Color.Parse("White");
    light.ListLow = Color.Parse("#ffdfeeaa");
    light.ListMedium = Color.Parse("#ffc2db65");
    light.RegionColor = Color.Parse("#fff7ffff");
    dark.Accent = Color.Parse("#ff34854d");
    dark.AltHigh = Color.Parse("Black");
    dark.AltLow = Color.Parse("Black");
    dark.AltMedium = Color.Parse("Black");
    dark.AltMediumHigh = Color.Parse("Black");
    dark.AltMediumLow = Color.Parse("Black");
    dark.BaseHigh = Color.Parse("White");
    dark.BaseLow = Color.Parse("#ff784834");
    dark.BaseMedium = Color.Parse("#ffc5a294");
    dark.BaseMediumHigh = Color.Parse("#ffd8b8ac");
    dark.BaseMediumLow = Color.Parse("#ff9e7564");
    dark.ChromeAltLow = Color.Parse("#ffd8b8ac");
    dark.ChromeBlackHigh = Color.Parse("Black");
    dark.ChromeBlackLow = Color.Parse("#ffd8b8ac");
    dark.ChromeBlackMedium = Color.Parse("Black");
    dark.ChromeBlackMediumLow = Color.Parse("Black");
    dark.ChromeDisabledHigh = Color.Parse("#ff784834");
    dark.ChromeDisabledLow = Color.Parse("#ffc5a294");
    dark.ChromeGray = Color.Parse("#ffb28b7c");
    dark.ChromeHigh = Color.Parse("#ffb28b7c");
    dark.ChromeLow = Color.Parse("#ff46150a");
    dark.ChromeMedium = Color.Parse("#ff532215");
    dark.ChromeMediumLow = Color.Parse("#ff6c3b2a");
    dark.ChromeWhite = Color.Parse("White");
    dark.ListLow = Color.Parse("#ff532215");
    dark.ListMedium = Color.Parse("#ff784834");
    dark.RegionColor = Color.Parse("#ff353819");
  }

  /// <summary>
  ///   Lavender palette preset from the online Fluent Editor https://theme.xaml.live/.
  /// </summary>
  private static void SelectLavenderColourScheme(
    ColorPaletteResources light,
    ColorPaletteResources dark) {
    light.Accent = Color.Parse("#ff8961cc");
    light.AltHigh = Color.Parse("White");
    light.AltLow = Color.Parse("White");
    light.AltMedium = Color.Parse("White");
    light.AltMediumHigh = Color.Parse("White");
    light.AltMediumLow = Color.Parse("White");
    light.BaseHigh = Color.Parse("Black");
    light.BaseLow = Color.Parse("#ffeeceff");
    light.BaseMedium = Color.Parse("#ffa987bc");
    light.BaseMediumHigh = Color.Parse("#ff7b5890");
    light.BaseMediumLow = Color.Parse("#ff9270a6");
    light.ChromeAltLow = Color.Parse("#ff7b5890");
    light.ChromeBlackHigh = Color.Parse("Black");
    light.ChromeBlackLow = Color.Parse("#ffeeceff");
    light.ChromeBlackMedium = Color.Parse("#ff7b5890");
    light.ChromeBlackMediumLow = Color.Parse("#ffa987bc");
    light.ChromeDisabledHigh = Color.Parse("#ffeeceff");
    light.ChromeDisabledLow = Color.Parse("#ffa987bc");
    light.ChromeGray = Color.Parse("#ff9270a6");
    light.ChromeHigh = Color.Parse("#ffeeceff");
    light.ChromeLow = Color.Parse("#fffeeaff");
    light.ChromeMedium = Color.Parse("#fffbe4ff");
    light.ChromeMediumLow = Color.Parse("#fffeeaff");
    light.ChromeWhite = Color.Parse("White");
    light.ListLow = Color.Parse("#fffbe4ff");
    light.ListMedium = Color.Parse("#ffeeceff");
    light.RegionColor = Color.Parse("#fffef6ff");
    dark.Accent = Color.Parse("#ff8961cc");
    dark.AltHigh = Color.Parse("Black");
    dark.AltLow = Color.Parse("Black");
    dark.AltMedium = Color.Parse("Black");
    dark.AltMediumHigh = Color.Parse("Black");
    dark.AltMediumLow = Color.Parse("Black");
    dark.BaseHigh = Color.Parse("White");
    dark.BaseLow = Color.Parse("#ff64576b");
    dark.BaseMedium = Color.Parse("#ffb6aabc");
    dark.BaseMediumHigh = Color.Parse("#ffcbbfd0");
    dark.BaseMediumLow = Color.Parse("#ff8d8193");
    dark.ChromeAltLow = Color.Parse("#ffcbbfd0");
    dark.ChromeBlackHigh = Color.Parse("Black");
    dark.ChromeBlackLow = Color.Parse("#ffcbbfd0");
    dark.ChromeBlackMedium = Color.Parse("Black");
    dark.ChromeBlackMediumLow = Color.Parse("Black");
    dark.ChromeDisabledHigh = Color.Parse("#ff64576b");
    dark.ChromeDisabledLow = Color.Parse("#ffb6aabc");
    dark.ChromeGray = Color.Parse("#ffa295a8");
    dark.ChromeHigh = Color.Parse("#ffa295a8");
    dark.ChromeLow = Color.Parse("#ff332041");
    dark.ChromeMedium = Color.Parse("#ff3f2e4b");
    dark.ChromeMediumLow = Color.Parse("#ff584960");
    dark.ChromeWhite = Color.Parse("White");
    dark.ListLow = Color.Parse("#ff3f2e4b");
    dark.ListMedium = Color.Parse("#ff64576b");
    dark.RegionColor = Color.Parse("#ff262738");
  }

  /// <summary>
  ///   Nighttime palette preset from the online Fluent Editor https://theme.xaml.live/.
  /// </summary>
  private static void SelectNighttimeColourScheme(
    ColorPaletteResources light,
    ColorPaletteResources dark) {
    light.Accent = Color.Parse("#ffcc4d11");
    light.AltHigh = Color.Parse("White");
    light.AltLow = Color.Parse("White");
    light.AltMedium = Color.Parse("White");
    light.AltMediumHigh = Color.Parse("White");
    light.AltMediumLow = Color.Parse("White");
    light.BaseHigh = Color.Parse("Black");
    light.BaseLow = Color.Parse("#ff7cbee0");
    light.BaseMedium = Color.Parse("#ff3282a8");
    light.BaseMediumHigh = Color.Parse("#ff005a83");
    light.BaseMediumLow = Color.Parse("#ff196e96");
    light.ChromeAltLow = Color.Parse("#ff005a83");
    light.ChromeBlackHigh = Color.Parse("Black");
    light.ChromeBlackLow = Color.Parse("#ff7cbee0");
    light.ChromeBlackMedium = Color.Parse("#ff005a83");
    light.ChromeBlackMediumLow = Color.Parse("#ff3282a8");
    light.ChromeDisabledHigh = Color.Parse("#ff7cbee0");
    light.ChromeDisabledLow = Color.Parse("#ff3282a8");
    light.ChromeGray = Color.Parse("#ff196e96");
    light.ChromeHigh = Color.Parse("#ff7cbee0");
    light.ChromeLow = Color.Parse("#ffc1e9fe");
    light.ChromeMedium = Color.Parse("#ffb3e0f8");
    light.ChromeMediumLow = Color.Parse("#ffc1e9fe");
    light.ChromeWhite = Color.Parse("White");
    light.ListLow = Color.Parse("#ffb3e0f8");
    light.ListMedium = Color.Parse("#ff7cbee0");
    light.RegionColor = Color.Parse("#ffcfeaff");
    dark.Accent = Color.Parse("#ffcc4d11");
    dark.AltHigh = Color.Parse("Black");
    dark.AltLow = Color.Parse("Black");
    dark.AltMedium = Color.Parse("Black");
    dark.AltMediumHigh = Color.Parse("Black");
    dark.AltMediumLow = Color.Parse("Black");
    dark.BaseHigh = Color.Parse("White");
    dark.BaseLow = Color.Parse("#ff2f7bad");
    dark.BaseMedium = Color.Parse("#ff8dbfdf");
    dark.BaseMediumHigh = Color.Parse("#ffa5d0ec");
    dark.BaseMediumLow = Color.Parse("#ff5e9dc6");
    dark.ChromeAltLow = Color.Parse("#ffa5d0ec");
    dark.ChromeBlackHigh = Color.Parse("Black");
    dark.ChromeBlackLow = Color.Parse("#ffa5d0ec");
    dark.ChromeBlackMedium = Color.Parse("Black");
    dark.ChromeBlackMediumLow = Color.Parse("Black");
    dark.ChromeDisabledHigh = Color.Parse("#ff2f7bad");
    dark.ChromeDisabledLow = Color.Parse("#ff8dbfdf");
    dark.ChromeGray = Color.Parse("#ff76aed3");
    dark.ChromeHigh = Color.Parse("#ff76aed3");
    dark.ChromeLow = Color.Parse("#ff093b73");
    dark.ChromeMedium = Color.Parse("#ff134b82");
    dark.ChromeMediumLow = Color.Parse("#ff266b9f");
    dark.ChromeWhite = Color.Parse("White");
    dark.ListLow = Color.Parse("#ff134b82");
    dark.ListMedium = Color.Parse("#ff2f7bad");
    dark.RegionColor = Color.Parse("#ff0d2644");
  }
}