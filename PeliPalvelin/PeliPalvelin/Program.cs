using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace PeliPalvelin
{
    class Program
    {
        static char[] erotin = { ' ' };
        static void Main(string[] args)
        {
            Socket palvelin;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 9999);
            try
            {
                palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                palvelin.Bind(iep);
            }
            catch
            {
                return;
            }

            String STATE = "WAIT";

            Boolean on = true;
            int vuoro = -1;
            int pelaajat = 0;
            int luku = -1;
            EndPoint[] pelaaja = new EndPoint[2];
            String[] Nimi = new String[2];
            Boolean pelaaja1_kuitannut = false;
            Boolean pelaaja2_kuitannut = false;

            while (on)
            {
                IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(client);
                String[] palat = Vastaanota(palvelin, ref remote);

                switch (STATE)
                {
                    case "WAIT":
                        switch (palat[0])
                        {
                            case "JOIN":
                                pelaaja[pelaajat] = remote;
                                Nimi[pelaajat] = palat[1];
                                pelaajat++;
                                switch (pelaajat)
                                {
                                    case 1:
                                        Laheta(palvelin, pelaaja[0], "ACK 201");
                                        break;
                                    case 2:
                                        Random rand = new Random();
                                        int Aloittaja = rand.Next(0, 1);
                                        vuoro = Aloittaja;
                                        luku = rand.Next(1, 10);
                                        Console.WriteLine("Oikea luku on: " + luku);
                                        Laheta(palvelin, pelaaja[Aloittaja], "ACK 202 " + Nimi[Flip(Aloittaja)]);
                                        Laheta(palvelin, pelaaja[Flip(Aloittaja)], "ACK 203 " + Nimi[Aloittaja]);
                                        STATE = "GAME";
                                        break;
                                    default:
                                        Laheta(palvelin, pelaaja[2], "2 pelaajaa on jo liittynyt!");
                                        break;
                                } //pelaajien maara
                                break;

                            default:
                                Laheta(palvelin, remote, "Palvelimelle lähettämäsi viesti oli virheellinen!");
                                break;
                        } //eka pala
                        break;
                    case "GAME":
                        switch(palat[0])
                        {
                            case "DATA":

                                if (remote.Equals(pelaaja[vuoro]))
                                {
                                    int arvaus = -1;
                                    if (int.TryParse(palat[1], out arvaus))
                                    {
                                        Laheta(palvelin, remote, "ACK 300");

                                        Laheta(palvelin, pelaaja[Flip(vuoro)], "DATA " + arvaus.ToString());
                                        


                                        if (luku == arvaus)
                                        {
                                            Laheta(palvelin, pelaaja[vuoro], "QUIT 501");
                                            Laheta(palvelin, pelaaja[Flip(vuoro)], "QUIT 502");
                                            STATE = "END";
                                            break;
                                        }
                                        STATE = "WAIT_ACK";
                                    }
                                    else
                                    {
                                        Laheta(palvelin, remote, "ACK 407 Syötit jotain muuta kuin luvun!");
                                    }
                                }
                                else
                                {
                                    Laheta(palvelin, remote, "ACK 402 Ei ole sinun vuorosi!");
                                }
                                break;
                        }
                        break;
                    case "WAIT_ACK":

                        switch (palat[0])
                        {
                            case "ACK":
                                switch (palat[1].Substring(0, 3))
                                {
                                    case "300":
                                        if (remote.Equals(pelaaja[Flip(vuoro)]))
                                        {
                                            vuoro = Flip(vuoro);
                                            Console.WriteLine("Vuoro vaihtuu");
                                            STATE = "GAME";
                                        }
                                        else
                                        {
                                            Laheta(palvelin, pelaaja[vuoro], "Odota että vastustajasi kuittaa..");
                                        }
                                        break;
                                    default:
                                        Laheta(palvelin, pelaaja[Flip(vuoro)], "ACK 403 Virheellinen ACK viesti 2");
                                        break;
                                }
                                break;
                            default:
                                Laheta(palvelin, pelaaja[Flip(vuoro)], "ACK 403 Virheellinen ACK viesti 1");
                                break;
                        }
                        
                        break;
                    case "END":
                        if (palat[0].Equals("ACK") && palat[1].Substring(0,3).Equals("500"))
                        {
                            if (remote.Equals(pelaaja[0])) pelaaja1_kuitannut = true;
                            if (remote.Equals(pelaaja[1])) pelaaja2_kuitannut = true;
                        }
                        if (pelaaja1_kuitannut && pelaaja2_kuitannut)
                        {
                            Console.WriteLine("Molemmat kuitanneet, lopetetaan");
                            on = false;
                        }
                        break;
                    default:
                        Console.WriteLine("Virhe...");
                        break;
                }
            }
            Console.ReadKey();
            palvelin.Close();
        }

        public static void Laheta(Socket palvelin, EndPoint asiakasEp, string viesti)
        {
            palvelin.SendTo(Encoding.ASCII.GetBytes(viesti), asiakasEp);
        }

        public static string[] Vastaanota(Socket palvelin, ref EndPoint remote)
        {
            byte[] rec = new byte[256];
            palvelin.ReceiveFrom(rec, ref remote);

            String rec_string = Encoding.ASCII.GetString(rec);
            String[] palat = rec_string.Split(erotin, 2);

            Console.WriteLine(palat[0]);
            Console.WriteLine(palat[1]);

            return palat;
        }
        public static int Flip(int i)
        {
            return 1 - i;
        }

    }
}
