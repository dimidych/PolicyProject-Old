using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Windows;
using System.Windows.Interop;

namespace PolicyProjectServiceHost
{
   [StructLayout(LayoutKind.Sequential)]
   public struct Margins
   {
      public int cxLeftWidth;
      public int cxRightWidth;
      public int cyTopHeight;
      public int cyBottomHeight;
   }

   public partial class MainWindow : Window
   {
      [DllImport("DwmApi.dll")]
      public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

      internal static ServiceHost UserService = null;

      public MainWindow()
      {
         InitializeComponent();
         DataContext = new PpsHostViewModel();
      }

      private static Margins GetDpiAdjustedMargins(IntPtr windowHandle, int left, int right, int top, int bottom)
      {
         Graphics gr = Graphics.FromHwnd(windowHandle);
         var dpiX = gr.DpiX;
         var dpiY = gr.DpiY;
         return new Margins
         {
            cxLeftWidth = Convert.ToInt32(left * (dpiX / 96)),
            cxRightWidth = Convert.ToInt32(right * (dpiX / 96)),
            cyTopHeight = Convert.ToInt32(top * (dpiY / 96)),
            cyBottomHeight = Convert.ToInt32(bottom * (dpiY / 96))
         };
      }

      public static void ExtendGlass(Window win, int left, int right, int top, int bottom)
      {
         var interopHelper = new WindowInteropHelper(win);
         var handle = interopHelper.Handle;
         var margins = GetDpiAdjustedMargins(handle, left, right, top, bottom);
         var hresult = DwmExtendFrameIntoClientArea(handle, ref margins);

         if (hresult < 0)
            throw new Exception("Operation failed");
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         try
         {
            //ExtendGlass(this, 5, 5, (int)TopBar.ActualHeight + 5, 5);
         }
         catch (Exception ex)
         {
            this.Background = System.Windows.Media.Brushes.White;
         }
      }
   }
}