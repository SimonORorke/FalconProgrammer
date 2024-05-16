using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using FalconProgrammer.Controls;
using JetBrains.Annotations;

namespace FalconProgrammer;

/// <summary>
///   Based on https://github.com/AvaloniaUtils/HyperText.Avalonia.
///   See <see cref="Hyperlink " /> summary.
/// </summary>
[ExcludeFromCodeCoverage]
[UsedImplicitly]
public class HyperLinkStyle : Styles, IResourceProvider, IStyle {
  private readonly IStyle _controlsStyles;
  private bool _isLoading;
  private IStyle? _loaded;

    /// <summary>
    ///   Initializes a new instance of the <see cref="HyperLinkStyle" /> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    [RequiresUnreferencedCode("Calls Avalonia.Markup.Xaml.Styling.StyleInclude.StyleInclude(Uri)")]
    private HyperLinkStyle(Uri? baseUri) {
    _controlsStyles = new StyleInclude(baseUri) {
      Source = new Uri("avares://HyperText.Avalonia/Styles/HyperlinkStyle.axaml")
    };
  }

    /// <summary>
    ///   Initializes a new instance of the <see cref="HyperLinkStyle" /> class.
    /// </summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    [RequiresUnreferencedCode("Calls Avalonia.Markup.Xaml.Styling.StyleInclude.StyleInclude(Uri)")]
    public HyperLinkStyle(IServiceProvider serviceProvider)
    : this(((IUriContext)serviceProvider.GetService(typeof(IUriContext))!).BaseUri) { }

  /// <summary>
  ///   Gets the loaded style.
  /// </summary>
  private IStyle Loaded {
    get {
      if (_loaded != null) {
        return _loaded!;
      }
      _isLoading = true;
      _loaded = new Styles { _controlsStyles };
      _isLoading = false;
      return _loaded!;
    }
  }

  public new IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

  public new bool TryGetResource(object key, ThemeVariant? theme, out object? value) {
    if (!_isLoading && Loaded is IResourceProvider p) {
      return p.TryGetResource(key, theme, out value);
    }
    value = null;
    return false;
  }

  bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

  public new event EventHandler? OwnerChanged {
    add {
      if (Loaded is IResourceProvider rp) {
        rp.OwnerChanged += value;
      }
    }
    remove {
      if (Loaded is IResourceProvider rp) {
        rp.OwnerChanged -= value;
      }
    }
  }

  void IResourceProvider.AddOwner(IResourceHost owner) {
    (Loaded as IResourceProvider)?.AddOwner(owner);
  }

  void IResourceProvider.RemoveOwner(IResourceHost owner) {
    (Loaded as IResourceProvider)?.RemoveOwner(owner);
  }

  IReadOnlyList<IStyle> IStyle.Children => _loaded?.Children ?? Array.Empty<IStyle>();
}