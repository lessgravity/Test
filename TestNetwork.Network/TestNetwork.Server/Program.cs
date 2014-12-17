
using System;
using System.Net;

namespace TestNetwork.Server
{
    static class Program
    {
        private static NetworkServer _server;

        static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
                if (_server != null)
                {
                    _server.Stop();
                }
            }
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleCancelKeyPress;

            _server = new NetworkServer();
            _server.Start(new IPEndPoint(IPAddress.Any, 0xBEEF));
            _server.Dispose();
        }
    }
}
