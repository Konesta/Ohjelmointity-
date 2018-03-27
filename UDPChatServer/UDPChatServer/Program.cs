using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace UDPChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket s = null;
            int port = 9999;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);
            List<EndPoint> asiakkaat = new List<EndPoint>();
            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Bind(iep);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Virhe: " + ex.Message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Odotetaan asiakasta...");

            while(!Console.KeyAvailable)
            {
                byte[] rec = new byte[256];
                IPEndPoint asiakas = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)asiakas;
                int received = s.ReceiveFrom(rec, ref remote);

                String rec_string = Encoding.ASCII.GetString(rec, 0, received);
                char[] delim = { ';' };
                String[] palat = rec_string.Split(delim, 2);
                if (palat.Length < 2)
                {
                    // lähetä virhe
                }
                else
                {
                    if (!asiakkaat.Contains(remote)) ;
                    {
                        asiakkaat.Add(remote);
                        Console.WriteLine("Uusi asiakas: [{0}:{1}]", ((IPEndPoint)remote).Address, ((IPEndPoint)remote).Port);
                    }
                    Console.WriteLine("{0}: {1}", palat[0], palat[1]);

                    foreach(EndPoint client in asiakkaat)
                    {
                        // String kaikille = palat[0] + ": " + palat[1];
                        s.SendTo(Encoding.ASCII.GetBytes(rec_string), client);
                    }
                }
            }

            Console.ReadKey();
            s.Close();
        }
    }
}
