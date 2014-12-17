using System;
using System.Net;
using System.Windows.Forms;

namespace TestNetwork.Network.Client
{
    public partial class MainForm : Form
    {
        private readonly NetworkClient _networkClient;
        private readonly NetworkSession _session;

        public MainForm()
        {
            InitializeComponent();

            _session = new NetworkSession
            {
                Name = "TestSession"
            };
            _networkClient = new NetworkClient(_session);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _networkClient.Connect(new IPEndPoint(IPAddress.Loopback, 0xBEEF));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _networkClient.Disconnect("Going To Bed...");
        }
    }
}
