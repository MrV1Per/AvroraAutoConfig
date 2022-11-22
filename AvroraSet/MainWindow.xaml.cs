using System.Windows;
using System.Windows.Input;
using System.Net;
using System;
using System.IO;
using System.Diagnostics;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AvroraSet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string dns = fieldDns.Text;
            string corpusNum = fieldNum.Text;
            string fieldInputIP = fieldIP.Text;

            string newDNS = dns.Replace('з', 'p'); 

            //Получить номер магазина и номер ПЧ из введенного ДНС имени
            Regex regexNumTT = new Regex(@"\d{4}");
            Regex regexNumPreice = new Regex(@"p\d{1,2}");
            string priceNum = regexNumPreice.Matches(newDNS)[0].ToString().Remove(0, 1);
            string numTT = regexNumTT.Matches(newDNS)[0].ToString();

            try
            {               
                string ip;
                if (SecondVariantCB.IsChecked == true)
                {

                    //Значения последниго сегмента ИП адреса прайсчекеров (1-50)
                    string[] segmentIP = { "", "50", "51", "52", "53", "54", "55", "56", "57", "58", "80", 
                                             "81", "82", "83", "84", "85", "86", "87", "88", "89", "90" ,
                                             "91", "92", "93", "94", "95", "96", "97", "98", "99","200", 
                                             "201", "202", "203", "204", "205", "206", "207", "208", "209", "210",
                                             "211", "212", "213", "214", "215", "216", "217", "218", "219", "220" };
                    string routerIp = Dns.GetHostEntry("a" + numTT + "-router").AddressList[0].ToString();

                    routerIp = routerIp.Remove(routerIp.Length - 1);
                    Int32.TryParse(priceNum, out int pNum);
                    ip = routerIp + segmentIP[pNum];

                }
                else if (ipCB.IsChecked == true)                      
                    ip = Dns.GetHostEntry(newDNS).AddressList[0].ToString(); //айпишник                   
                else
                    ip = fieldInputIP;
                

                for (; ; )
                {
                    if (numTT.StartsWith("0")) numTT = numTT.TrimStart('0');
                    break;
                }


                new Window1(numTT, priceNum.ToString(), ip, corpusNum, newDNS).ShowDialog();

            }
            catch(System.Net.Sockets.SocketException)
            {
                MessageBox.Show("Нужный DNS адрес не найден, возможно не правильно введена информация, или запрашиваемый DNS не прописан в сети", "ERROR");
                //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                //Application.Current. Shutdown();
            }


        }

        /// <summary>
        /// Закрытие приложения
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Возможность двигать окно за топ меню
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Открытие файла с логами работы приложения
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Button_Log(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", @"..\AvroraAutoConfig\log.txt");
        }

        /// <summary>
        /// Внести данные в файл конфига
        /// </summary>
        /// <param name="line">Строка считаная с конфига</param>
        /// <param name="ip">ИП адрес устройства</param>
        /// <param name="gateway">Шлюз</param>
        /// <returns></returns>
        private string ParseInputCFG(string line, string ip, string gateway)
        {
            return line.Replace("{IP}", ip)
                .Replace("{DNS}", fieldDns.Text)
                .Replace("{GATEWAY}", gateway)
                .Replace("{SERIAL}", SerialNum.Text); //ИП

        }

        /// <summary>
        /// Обработка нажатия кнопки "Orange"
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void Orange_Click(object sender, RoutedEventArgs e)
        {
            //Данные для подключения по SSH
            string user = "pi";
            //string passDoor = GetPassByLogin(user);   :TODO
            string passMusic = "";
            string host = fieldLocalIP.Text;


            string pathToConfigTemplateOrange = @"..\AvroraAutoConfig\music.py"; //Путь к шаблонному файлу конига оранжа
            List<string> musicCfg = new List<string>();

            using (StreamReader sr = new StreamReader(pathToConfigTemplateOrange, Encoding.Default))
            {
                string ip = Dns.GetHostEntry(fieldDns.Text).AddressList[0].ToString(); //Получить ИП адрес по ДНС имени
                string gateway = ip.Remove(ip.Length - 2) + "1";

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = ParseInputCFG(line, ip, gateway);
                    musicCfg.Add(line);
                }
            }


            string pathOut = @"..\AvroraAutoConfig\musicFull.py"; //Путь к исходящему файлу измененного конфига
            using (StreamWriter wr = new StreamWriter(pathOut, false, System.Text.Encoding.Default))//создаем файл с полним конфигом
            {
                int i = 0;
                while (i < musicCfg.Count)
                {
                    wr.WriteLine(musicCfg[i]);
                    i++;
                }

            }

            //создаем sftp подключение, загружаем созданый файл на устрофство
            using (ScpClient client = new ScpClient(host, user, passMusic))
            {
                client.Connect();
                Thread.Sleep(200);
                using (Stream localFile = File.OpenRead(pathOut))
                {
                    client.Upload(localFile, "/home/pi/orangeAuto.py");
                }

            }

            //Подключение по SSH, и запуск скрипта для настройки
            using (var client = new SshClient(host, user, passMusic))
            {
                string dns = fieldDns.Text;
                string newDNS = dns.Replace('з', 'p');
                //Start the connection
                try
                {
                    client.Connect();
                    Thread.Sleep(1000);
                    client.RunCommand("echo 'Pi@)!^' | sudo -S python3 orangeAuto.py");

                    Thread.Sleep(200);
                    client.RunCommand("rm orangeAuto.py");

                    Thread.Sleep(10);
                    client.RunCommand($"echo '{passMusic}' | sudo -S reboot");

                    client.Disconnect();
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error" + ex);
                }
            }
        }


        /// <summary>
        /// Обработка нажатия кнопки "Door"
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void DD_Click(object sender, RoutedEventArgs e)
        {
            
            //Данные для подключения по SSH
            string user = "pi";
            //string passDoor = GetPassByLogin(user);   :TODO
            string passDoor = "";
            string host = fieldLocalIP.Text;


            string pathToConfigTemplateDoor = @"..\AvroraAutoConfig\door.py"; //Путь к шаблонному файлу конига датчика дверей
            List<string> doodDeviceConfig = new List<string>();


            using (StreamReader sr = new StreamReader(pathToConfigTemplateDoor, Encoding.Default))
            {
                string ip = Dns.GetHostEntry(fieldDns.Text).AddressList[0].ToString(); //Получить ИП адрес по ДНС имени
                string gateway = ip.Remove(ip.Length - 2) + "1";

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = ParseInputCFG(line, ip, gateway);
                    doodDeviceConfig.Add(line);
                }
            }


            string pathOut = @"..\AvroraAutoConfig\doorFull.py";
            using (StreamWriter wr = new StreamWriter(pathOut, false, System.Text.Encoding.Default))//создаем файл с полним конфигом
            {
                int i = 0;
                while (i < doodDeviceConfig.Count)
                {
                    wr.WriteLine(doodDeviceConfig[i]);
                    i++;
                }

            }
            //создаем sftp подключение, загружаем созданый файл на устрофство
            using (ScpClient client = new ScpClient(host, user, passDoor))
            {
                client.Connect();
                using (Stream localFile = File.OpenRead(pathOut))
                {
                    client.Upload(localFile, "/home/pi/doorAuto.py");
                }

            }
            //Подключение по SSH, и запуск скрипта для настройки
            using (var client = new SshClient(host, user, passDoor))
            {              
                try
                {
                    client.Connect();
                    Thread.Sleep(1000);

                    var Request = client.RunCommand($@"echo '{passDoor}' | sudo -S python3 doorAuto.py");
                    Thread.Sleep(100);
                    client.RunCommand($@"echo '{passDoor}' | sudo -S reboot");

                    Thread.Sleep(500);
                    client.Disconnect();
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error" + ex);
                }
            }           
        }            
    }
}
