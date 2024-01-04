namespace FalconProgrammer;

public partial class App : Application {
  public App() {
    InitializeComponent();
    MainPage = new AppShell();
  }

// #if WINDOWS 
//   /// <summary>
//   ///   This was proposed at
//   ///   https://stackoverflow.com/questions/77740217/stop-a-maui-application-from-showing-a-wait-cursor-when-loaded-in-windows,
//   ///   as an answer to the question I asked about how to stop a Maui application from
//   ///   showing a wait cursor when loaded in Windows. The proposed answer was withdrawn
//   ///   when I explained that the suggested solution did not work properly for me. 
//   /// </summary>
//   /// <remarks>
//   ///   If I run within Visual Studio this works for me, though sometimes I see both the
//   ///   wait cursor and the default cursor together (a "busy" cursor?) for a few seconds
//   ///   before the default cursor is shown. But tha happens even when this is commented
//   ///   out. If I run within the JetBrains Rider IDE or
//   ///   run the deployed application outside the context of an IDE, it does not work.
//   ///   <para>
//   ///     Also, the '#if WINDOWS' preprocessor directive does not work for me in Rider. I
//   ///     need to get thar fixed of I'm to use this CreateWindow and its associated
//   ///     Windows API declarations below.
//   ///   </para>
//   /// </remarks>
//   protected override Window CreateWindow(IActivationState? activationState) {
//     var window = base.CreateWindow(activationState);
//     if (DeviceInfo.Platform == DevicePlatform.WinUI) {
//       window.HandlerChanged += (sender, e) => {
//         if (sender is Window { Handler: not null }) {
//           Dispatcher.Dispatch(() => {
//             if (GetCursorPos(out var currentPos)) {
//               SetCursorPos(currentPos.X + 1, currentPos.Y + 1);
//             }
//           });
//         }
//       };
//     }
//     return window;
//   }
//   
//   [DllImport("user32.dll")]
//   private static extern bool SetCursorPos(int X, int Y);
//   
//   [DllImport("user32.dll")]
//   [return: MarshalAs(UnmanagedType.Bool)]
//   private static extern bool GetCursorPos(out POINT lpPoint);
//   
//   [StructLayout(LayoutKind.Sequential)]
//   public struct POINT {
//     public int X;
//     public int Y;
//   }
// #endif
}