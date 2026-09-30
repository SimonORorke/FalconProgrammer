using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Platform;

namespace FalconProgrammer;

/// <summary>
/// Facilitates setting the application icon in macOS Dock and Application Switcher.
/// </summary>
internal static class MacIconHelper {
  private const string ObjCRuntime = "/usr/lib/libobjc.A.dylib";

  [DllImport(ObjCRuntime, EntryPoint = "objc_getClass")]
  private static extern nint objc_getClass(string className);

  [DllImport(ObjCRuntime, EntryPoint = "sel_registerName")]
  private static extern nint sel_registerName(string selectorName);

  [DllImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
  private static extern nint objc_msgSend_IntPtr(nint receiver, nint selector);

  [DllImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
  private static extern nint objc_msgSend_IntPtr_IntPtr(nint receiver, nint selector, nint arg1);

  [DllImport(ObjCRuntime, EntryPoint = "objc_msgSend")]
  private static extern nint objc_msgSend_IntPtr_IntPtr_nuint(
    nint receiver, nint selector, nint arg1, nuint arg2);

  [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
  private static extern void NSApplicationLoad();

  /// <summary>
  /// Sets the application icon in the macOS Dock and Application Switcher using the .icns asset.
  /// </summary>
  public static void SetApplicationIcon() {
    if (!OperatingSystem.IsMacOS()) {
      return;
    }

    try {
      var uri = new Uri("avares://FalconProgrammer/Assets/falcon_svg_repo_com_512_512.icns");
      if (!AssetLoader.Exists(uri)) {
        return;
      }

      using var stream = AssetLoader.Open(uri);
      using var memoryStream = new MemoryStream();
      stream.CopyTo(memoryStream);
      byte[] iconData = memoryStream.ToArray();

      if (iconData.Length == 0) {
        return;
      }

      NSApplicationLoad();

      nint nsApplicationClass = objc_getClass("NSApplication");
      if (nsApplicationClass == 0) {
        return;
      }

      nint sharedApplicationSel = sel_registerName("sharedApplication");
      nint nsApp = objc_msgSend_IntPtr(nsApplicationClass, sharedApplicationSel);
      if (nsApp == 0) {
        return;
      }

      nint nsDataClass = objc_getClass("NSData");
      nint dataWithBytesSel = sel_registerName("dataWithBytes:length:");

      var handle = GCHandle.Alloc(iconData, GCHandleType.Pinned);
      try {
        nint pBytes = handle.AddrOfPinnedObject();
        nint nsData = objc_msgSend_IntPtr_IntPtr_nuint(
          nsDataClass, dataWithBytesSel, pBytes, (nuint)iconData.Length);
        if (nsData == 0) {
          return;
        }

        nint nsImageClass = objc_getClass("NSImage");
        nint allocSel = sel_registerName("alloc");
        nint initWithDataSel = sel_registerName("initWithData:");

        nint allocatedImage = objc_msgSend_IntPtr(nsImageClass, allocSel);
        nint nsImage = objc_msgSend_IntPtr_IntPtr(allocatedImage, initWithDataSel, nsData);
        if (nsImage == 0) {
          return;
        }

        nint setApplicationIconImageSel = sel_registerName("setApplicationIconImage:");
        objc_msgSend_IntPtr_IntPtr(nsApp, setApplicationIconImageSel, nsImage);

        nint releaseSel = sel_registerName("release");
        objc_msgSend_IntPtr(nsImage, releaseSel);
      } finally {
        handle.Free();
      }
    } catch {
      // Gracefully ignore if native platform interop is unavailable
    }
  }
}
