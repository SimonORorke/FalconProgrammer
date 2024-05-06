using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace FalconProgrammer;

public static class Converters {
  /// <summary>
  ///   Converters an image file path to a <see cref="Bitmap" /> that can be bound to an
  ///   <see cref="Image" />'s <see cref="Image.Source" />.
  /// </summary>
  public static FuncValueConverter<string?, Bitmap?> PathToBitmap { get; } =
    new FuncValueConverter<string?, Bitmap?>(path =>
      !string.IsNullOrEmpty(path) ? new Bitmap(path) : null);
}