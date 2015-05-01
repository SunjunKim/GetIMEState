using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Timers;

namespace getImeStatus
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        // Declare external functions.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd,
                                                StringBuilder text,
                                                int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        const uint WM_IME_CONTROL = 0x0283;

        // REFERENCE = https://indidev.net/forum/viewtopic.php?p=1089
        public void getIMEStatus()
        {
            IntPtr hWnd = GetForegroundWindow();
            IntPtr _hWndIme = ImmGetDefaultIMEWnd(hWnd);

            String title = GetActiveWindowTitle();

            COPYDATASTRUCT datastruct = new COPYDATASTRUCT();
            IntPtr ret = SendMessage(_hWndIme, WM_IME_CONTROL, 0x05, ref datastruct);

            if (ret == IntPtr.Zero)
            {
                // 0이면 영문  1 이면 한글
                SetText(display, title + "\nEnglish");
            }
            else
            {
                SetText(display, title + "\n한글");
            }
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public MainWindow()
        {
            Timer t = new Timer(500);
            t.Elapsed += t_Elapsed;
            t.Start();
            InitializeComponent();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            getIMEStatus();
        }

        private delegate void SetTextCallback(System.Windows.Controls.TextBox control, string text);

        // Thread safe updating of control's text property
        private void SetText(System.Windows.Controls.TextBox control, string text)
        {
            if (control.Dispatcher.CheckAccess())
            {
                control.Text = text;
            }
            else
            {
                SetTextCallback d = new SetTextCallback(SetText);
                control.Dispatcher.Invoke(d, new object[] { control, text });
            }
        }
    }
}
