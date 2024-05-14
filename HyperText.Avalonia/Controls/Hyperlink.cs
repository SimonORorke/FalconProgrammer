using Avalonia;
using Avalonia.Controls;

namespace HyperText.Avalonia.Controls;

/// <summary>
///   Based on https://github.com/AvaloniaUtils/HyperText.Avalonia.
///   There's a HyperText.Avalonia NuGet package. But that does not work for me.
///   I reported the problem:
///   https://github.com/AvaloniaUtils/HyperText.Avalonia/issues/4.
/// </summary>
public class Hyperlink : Button {
  public static readonly DirectProperty<Hyperlink, string> UrlProperty
    = AvaloniaProperty.RegisterDirect<Hyperlink, string>(nameof(Url), o => o.Url,
      (o, v) => o.Url = v);

  public static readonly DirectProperty<Hyperlink, string> AliasProperty
    = AvaloniaProperty.RegisterDirect<Hyperlink, string>(nameof(Alias), o => o.Alias,
      (o, v) => o.Alias = v);

  private string _alias = string.Empty;

  private string _url = string.Empty;

  public string Url {
    get => _url;
    set {
      SetAndRaise(UrlProperty, ref _url, value);
      var textBlock = new TextBlock {
        Text = _url
      };
      if (string.IsNullOrEmpty(_alias)) {
        Content = textBlock;
      }
      if (!string.IsNullOrEmpty(_url)) {
        Classes.Add("hyperlink");
      }
    }
  }

  public string Alias {
    get => string.IsNullOrEmpty(_alias) ? _url : _alias;
    set {
      SetAndRaise(UrlProperty, ref _alias, value);
      var textBlock = new TextBlock {
        Text = string.IsNullOrEmpty(_alias) ? _url : _alias
      };
      Content = textBlock;
    }
  }
}