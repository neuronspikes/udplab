using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Exocortex.DSP;

namespace UDPSpectrum

{
    class Program
    {
        static void Main(string[] args)
        {
            bool showUsage = true;
            if (args.Length >= 3)
            {
                try
                {
                    // todo: generalize port parsing and throw new exceptions
                    int inputPort = int.Parse(args[0]);
                    int amplitudeOutputPort = int.Parse(args[1]);
                    int phaseOutputPort = int.Parse(args[2]);
                    if (    inputPort > 0 && inputPort < 65536
                        && amplitudeOutputPort > 0 && amplitudeOutputPort < 65536
                        && phaseOutputPort > 0 && phaseOutputPort < 65536
                        )
                    {
                        showUsage = false;

                        String adrs = "127.0.0.1";
                        if (args.Length >= 4) adrs = args[3];
                        // todo: generalize address parsing and throw new exceptions
                        Console.WriteLine("Sending on address " + adrs);
                        Console.WriteLine("Receiving UDP packets on port " + inputPort);
                        Console.WriteLine("Sending amplitude UDP packets on port " + amplitudeOutputPort);
                        Console.WriteLine("Sending phase UDP packets on port " + phaseOutputPort);
                        UdpClient inputUdpClient = new UdpClient(inputPort);
                        UdpClient amplitudeUdpClient = new UdpClient(adrs, amplitudeOutputPort);
                        UdpClient phaseUdpClient = new UdpClient(adrs, phaseOutputPort);

                        try
                        {
                            while (true)
                            {

                                
                                //IPEndPoint object will allow us to read datagrams sent from any source.
                                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                                // Blocks until a message returns on this socket from a remote host.
                                Byte[] receiveBytes = inputUdpClient.Receive(ref RemoteIpEndPoint);

                                //todo:Size handling!!!
                                int size = 1024;

                                // prepare dataset from real data
                                List<ComplexF> cxList = new List<ComplexF>(size);
                                for (int i = 0; i < size; i++)
                                {
                                    // unsigned byte to R=[0..1],I=0 complex number
                                    cxList.Add(new ComplexF((float)(receiveBytes[i]-128)/128, 0));
                                }

                                // Transform in the complex array
                                ComplexF[] cxArray =cxList.ToArray<ComplexF>();
                                Fourier.FFT(cxArray, FourierDirection.Forward);
                                
                                // remap type
                                List<byte> abList = new List<byte>(size);
                                List<byte> pbList = new List<byte>(size);
                                for (int i = 0; i < size; i++)
                                {
                                    abList.Add((byte)(cxArray[i].GetModulus()*256));// Amplitude [0..256]
                                    pbList.Add((byte)(cxArray[i].GetArgument()/(2*Math.PI)*256));// Phase [0..256]
                                    
                                }
                                byte[] amplitude = abList.ToArray<Byte>();
                                byte[] phase = pbList.ToArray<Byte>();

                                // and send!
                                amplitudeUdpClient.Send(amplitude, amplitude.Length);
                                phaseUdpClient.Send(phase, phase.Length);
                            }


                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        finally
                        {
                            inputUdpClient.Close();
                            amplitudeUdpClient.Close();
                            phaseUdpClient.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    // will show usage anyway...
                    Console.WriteLine(e.ToString());
                    // todo: a friendly detailed explanation of the failure should be displayed when failing
                }
            }
            if (showUsage)
            {
                Console.WriteLine("Compute spectrum. using FFT");
                Console.WriteLine("USAGE: UDPSpectrum inputPort amplitudeOutputPort phaseOutputPort [sendToAddress]\n");
                Console.WriteLine("Warning: This kind of transformation is don on 2^x sets, som truncation and padding may occurs!");
                // todo: make a strict flag in the arguments to optionnaly reject non-conform packets
                Console.WriteLine("Hit any key to exit");
                Console.Read();
            }
        }
    }
}