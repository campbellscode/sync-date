using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SyncDate
{
    class Program
    {
        private static string networkTime;
        private static string formatNetworkDate;
        private static string machineTime;
        private static string formatMachineDate;

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello");
            networkTime = GetNetworkTime().ToString();
            VerifyDate(networkTime);
        }

        public static DateTime GetNetworkTime()
        {
            const string ntpServer = "pool.ntp.org";
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveTimeout = 10000;
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            try
            {
                socket.Receive(ntpData);
            }
            catch
            {
                GetNetworkTime();
                //MessageBox.Show("timed out");
                socket.Close();
            }
            socket.Close();
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime;
        }

        public static void VerifyDate(string networkTime)
        {
            // Have to trim the date based on how long it is.
            // Could be #/#/####, #/##/####, or ##/##/####.
            var nlen = networkTime.Length;

            if (nlen >= 21)
            {
                formatNetworkDate = networkTime.Remove(10).Trim();
            }
            if (nlen == 20)
            {
                formatNetworkDate = networkTime.Remove(9).Trim();
            }
            if (nlen < 20)
            {
                formatNetworkDate = networkTime.Remove(8).Trim();
            }

            machineTime = DateTime.Now.ToString();
            var mlen = machineTime.Length;
            if (mlen >= 21)
            {
                formatMachineDate = machineTime.Remove(10).Trim();
            }
            if (mlen == 20)
            {
                formatMachineDate = machineTime.Remove(9).Trim();
            }
            if (mlen < 20)
            {
                formatMachineDate = machineTime.Remove(8).Trim();
            }

            if (formatMachineDate != formatNetworkDate)
            {
                //MessageBox.Show("Incorrect date...fixing.");
                Console.WriteLine("Incorrect date...fixing.");

                var psi = new ProcessStartInfo();
                psi.FileName = @"C:\Windows\System32\cmd.exe";
                psi.Verb = "runas";
                psi.Arguments = "/C date " + formatNetworkDate;
                psi.UseShellExecute = true;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi);
            }
            else
            {
                //MessageBox.Show("Date verified.");
                Console.WriteLine("Date verified.");
            }
        }
    }
}

