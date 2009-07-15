using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;


namespace UDPSoundSender
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showUsage = true;

            if (args.Length >= 2) // requires 2 mandatory parameters: adrs + port
            {
                try
                {
                    String adrs = args[0];
                    // todo: validate with a regex (ip or name)

                    int port = int.Parse(args[1]);
                    if (port > 0 && port < 65536)
                    {
                        showUsage = false;

                        
                        // todo: retreive list of devices and select from/display it in Show Usage
                        SoundDeviceType device = SoundDeviceType.Default;

                        int samplesPerSecond = 11000;
                        short bitsPerSample = 8;
                        short channels = 1;
                        // todo: parse and set these parameters
                        SampleRate rate = SampleRate.Rate11KHz;
                        SampleSize size = SampleSize.Bits8;


                        SoundRecorder recorder = new SoundRecorder(SoundDeviceType.Default, rate, size, channels);
                        UdpClient udpClient = new UdpClient(adrs,port);
                        Console.WriteLine("Sending sound packets on UDP port " + port);
                        try
                        {
                            recorder.Start("");
                            while (recorder.Capturing())
                            {
                                //Sit here and wait for a message to arrive
                                recorder.NotificationEvent.WaitOne(System.Threading.Timeout.Infinite, true);

                                recorder.SendCapturedData(udpClient);
                            }


                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.WriteLine("Hit any key to continue");
                            Console.ReadKey();
                        }
                        finally
                        {
                            recorder.Stop();
                            udpClient.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if (showUsage)
            {
                Console.WriteLine("Send direct sound data as UDP packets.");
                Console.WriteLine("USAGE: UDPSoundSender targetIP port [8000 8 1]");
                Console.WriteLine("where:");
                Console.WriteLine("  targetIP is address where to send the packets (yes, multicast addresses should works!)");
                Console.WriteLine("  port is the one of the target ip listening");
                Console.WriteLine("  [optional parameters]");
                Console.WriteLine("  8000 is the default sample rate (choose 8000, 11025, 22050, 44100 or 48000 sample/sec)");
                Console.WriteLine("  8 is the default bit depth (choose 8 or 16 bits)");
                Console.WriteLine("  1 is the channels selected (choose 1 or 2 channels)");



                Console.WriteLine("Hit any key to exit");
                Console.Read();
            }
        }
    }
}
