using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        public static DateTime GetNetworkTime()
        {
            //default windows time server
            const string ntpServer = "time.windows.com";

            //NTP message size - 16 bytes of the digest (RFC 2030)
            byte[] ntpData = new byte[48];

            //setting the leap indicator, version number and mode values
            ntpData[0] = 0x1B;

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //the udp port number as signed to ntp os 37(123 xD)
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);

            //ntp uses udp
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //stop code hang if ntp is blocked
                socket.ReceiveTimeout = 3000;
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            const byte serverReplyTime = 40;

            //get the second part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //get the secionds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //utc time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) + ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) + ((x & 0xff000000) >> 24));
        }

        static void Main(string[] args)
        {            
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 37);
            Console.WriteLine("Server is host on {0}",RemoteIpEndPoint);
            try
            {
                while (true)
                {
                    string returnData = null;
                    string temp = null;
                    Byte[] receiveBytes = null;
                    UdpClient udpClient = new UdpClient(37);
                    while (true)
                    {
                        receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                        temp = GetNetworkTime().ToString();
                        returnData = temp + "<ServerInfo>";                        
                        Console.WriteLine("The time value: " + temp.ToString());
                        if (returnData.IndexOf("<ServerInfo>") > -1)
                        {
                            break;
                        }
                    }
                    byte[] msg = Encoding.ASCII.GetBytes(temp);
                    udpClient.Send(msg, msg.Length, RemoteIpEndPoint);
                    udpClient.Close();
                    Console.WriteLine("The data have been succesfully send to client.");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
