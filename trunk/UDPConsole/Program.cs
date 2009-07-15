using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UDPConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showUsage = true;
            if (args.Length > 0)
            {
                try
                {
                    int port = int.Parse(args[0]);
                    if (port > 0 && port < 65536)
                    {
                        showUsage = false;
                        UdpClient udpClient = new UdpClient(port);
                        Console.WriteLine("Receiving UDP packets on port " + port);
                        try
                        {
                            while (true)
                            {
                                //IPEndPoint object will allow us to read datagrams sent from any source.
                                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                                // Blocks until a message returns on this socket from a remote host.
                                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);

                                // Uses the IPEndPoint object to determine which of these two hosts responded.

                                Console.WriteLine("From " +
                                                            RemoteIpEndPoint.Address.ToString() +
                                                            ":" +
                                                            RemoteIpEndPoint.Port.ToString());
                                Console.WriteLine(ToHexString(receiveBytes));
                            }


                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        finally
                        {
                            udpClient.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            if (showUsage)
            {
                Console.WriteLine("Trace an UDP port activity. Displays content in hexadecimal");
                Console.WriteLine("USAGE: UDPConsole port\n");
                Console.WriteLine("Hit any key to exit");
                Console.Read();
            }
        }

        static char[] hexDigits = {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        public static string ToHexString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }


    }
}
