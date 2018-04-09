using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static UdpClient udpClient = new UdpClient();
        static byte[] bytes = new byte[1024];
        static IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        static IPAddress ipAddress = ipHostInfo.AddressList[0];
        static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 37);

        static void Main(string[] args)
        {            
            udpClient.Connect(Dns.GetHostName(), 37);
            Console.WriteLine("Client is connected to {0}", remoteEP);
            byte[] msg = { 1 };
            int bytesSent = udpClient.Send(msg, msg.Length);
            byte[] bytesRec = udpClient.Receive(ref remoteEP);
            Console.WriteLine("The time is :" + Encoding.ASCII.GetString(bytesRec, 0, bytesRec.Length));         
        }
    }
}
