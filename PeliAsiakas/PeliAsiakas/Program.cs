using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace PeliAsiakas
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 9999);

            EndPoint pep = (IPEndPoint)ep;

            Laheta(palvelin, pep, "JOIN Konsta");

            Boolean on = true;
            String tila = "JOIN";
            while(on)
            {
                String[] palat = Vastaanota(palvelin);
                switch(tila)
                {
                    // tarkista palojen määrä
                    case "JOIN":
                        switch(palat[0])
                        {
                            case "ACK":
                                switch(palat[1])
                                {
                                    case "201":
                                        Console.WriteLine("Odotetaan toista pelaajaa");
                                        break;
                                    case "202":
                                        Console.WriteLine("Vastustajasi on: " + palat[2]);
                                        Console.WriteLine("Anna numero");
                                        String luku = Console.ReadLine();
                                        Laheta(palvelin, pep, "DATA " + luku);
                                        tila = "GAME";
                                        break;
                                    case "203":
                                        Console.WriteLine("Vastustajasi " + palat[2] + " saa aloittaa");
                                        tila = "GAME";
                                        break;
                                } // viestin toinen osio
                                break;
                            default:
                                String viesti = palat[0] + palat[1];
                                Console.WriteLine("Virhe: " + viesti);
                                break;
                        } // viestin ensimmäinen osio
                        break;
                    case "GAME":
                        switch(palat[0])
                        {
                            case "ACK":
                                    switch(palat[1])
                                {
                                    case "300":
                                        Console.WriteLine("Vastaus lähetetty");
                                        break;
                                }
                                break;
                            case "DATA":
                                Console.WriteLine("Vastustajasi arvasi: " + palat[1]);
                                Laheta(palvelin, pep, "ACK 300");
                                Console.WriteLine("Anna numero");
                                String luku = Console.ReadLine();
                                Laheta(palvelin, pep, "DATA " + luku);
                                break;
                            case "QUIT":
                                switch (palat[1])
                                {
                                    case "501":
                                        Console.WriteLine("Sinä voitit! Peli päättyy.");
                                        Laheta(palvelin, pep, "ACK 500");
                                        break;
                                    case "502":
                                        Console.WriteLine("Hävisit pelin! Peli päättyy.");
                                        Laheta(palvelin, pep, "ACK 500");
                                        break;
                                }
                                break;
                        }
                        break;
                } //tila
            }
            palvelin.Close();
        }

        private static string[] Vastaanota(Socket palvelin)
        {
            palvelin.ReceiveTimeout = 100000;
            byte[] rec = new byte[256];
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint palvelinEp = (EndPoint)remote;
            string[] palat = {"", ""};
            try
            {
                palvelin.ReceiveFrom(rec, ref palvelinEp);
                string viesti = Encoding.ASCII.GetString(rec);
                char[] erotin = { ' ' };
                palat = viesti.Split(erotin, 3);
            }
            catch
            {
                //timeout
                Console.WriteLine("Timed out");
            }
        
            return palat;
        }

        public static void Laheta(Socket palvelin, EndPoint pep, string viesti)
        {
            palvelin.SendTo(Encoding.ASCII.GetBytes(viesti), pep);
        }
    }
}
