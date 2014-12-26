using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;


namespace Server
{
    class Program
    {
        static void Main()
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(data);

                    var results = Regex.Split(data, " ");
                    if (results.Count() == 3)
                    {
                        string reply = GetFileData(results[0], Convert.ToInt32(results[1]), Convert.ToInt32(results[2]));
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);

                        if (data.IndexOf("exit", StringComparison.Ordinal) > -1)
                        {
                            Console.WriteLine("Сервер завершил соединение с клиентом.");
                            break;
                        }
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        public static string GetFileData(string path, int offset, int count)
        {
            string reply = String.Empty;
            try
            {
                byte[] buffer = new byte[1024];
                if (File.Exists(path))
                {
                    File.OpenRead(path).Read(buffer, Convert.ToInt32(offset), Convert.ToInt32(count));

                   
                    reply = Encoding.Default.GetString(buffer).Trim('\0');
                }
                else
                {
                    reply = "Файл не найден";
                }
                return reply;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return reply;
            }
            
        }
    }
}
