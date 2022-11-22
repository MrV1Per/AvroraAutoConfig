using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;

namespace AvroraSet
{
    public partial class Window1 : Window
    {
        BarcodeWriter writer = new BarcodeWriter() { Format = BarcodeFormat.CODE_128 };
        string settings = "772211002";
        string saveSettings = "772211003";
        string cancel = "772211004";
        string infoBarcode = "772211001";
        string[] info = new string[8];
        string dns = "";
        int i = 0;
        public Window1(string numTT, string priceNum, string ip, string corpusNum, string inDns)
        {
            InitializeComponent();
            dns = inDns;
            bool isPing = IsInDomain(dns);

            isInDomain.Content = isPing ? "dns происан в домене" : "dns прдварительно не происан в домене, проверьте пожалуйста";

            info[1] = settings;
            info[2] = numTT;
            info[3] = priceNum;
            info[4] = ip;
            info[5] = corpusNum;
            info[6] = saveSettings;
        }

        /// <summary>
        /// Генерация изображения из ШК
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        /// <summary>
        /// Проверка, прописано ли устройство в домене
        /// </summary>
        /// <param name="nameOrAddress">ДНС имя</param>
        /// <returns></returns>
        public static bool IsInDomain(string nameOrAddress)
        {
            try
            {
                string ip = Dns.GetHostEntry(nameOrAddress).AddressList[0].ToString();
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// Обработка нажатия кнопки next (Пролистать слайдер вперед)
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (i <= 5)
            {
                i++;
            }
            else
            {
                using (StreamWriter w = new StreamWriter(@"..\AvroraAutoConfig\log.txt", true))
                {
                    w.WriteLine(DateTime.Now + " | " + dns + " | " + info[4] + " | " + info[5]);
                }

                Close();
            }
            pic.Source = BitmapToImageSource(writer.Write(info[i]));
        }

        /// <summary>
        /// Обработка нажатия кнопки Cancel (отобразить ШК для отмены настроек)
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            pic.Source = BitmapToImageSource(writer.Write(cancel));
        }

        /// <summary>
        /// Обработка нажатия кнопки Info
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            i = 0;
            pic.Source = BitmapToImageSource(writer.Write(infoBarcode));
        }

        /// <summary>
        /// Обработка нажатия кнопки Back (Пролистать слайдер назад)
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (i >= 2) i--;
            pic.Source = BitmapToImageSource(writer.Write(info[i]));
        }

        /// <summary>
        /// Обработка нажатия кнопки ping (Открывает консоль, и начинает пинговать введенный днс адрес)
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click_Ping(object sender, RoutedEventArgs e)
        {
            string pingData = "ping " + dns;

            var proc1 = new ProcessStartInfo();
            proc1.UseShellExecute = true;

            proc1.WorkingDirectory = @"C:\Windows\System32";

            proc1.FileName = @"C:\Windows\System32\cmd.exe";
            proc1.Verb = "runas";
            proc1.Arguments = "/K " + pingData;
            proc1.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(proc1);
        }
    }
}
