using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;


namespace testGit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = false;
            label4.Text = "Listening";
            string recieve_path = Application.StartupPath + "\\RECIEVE\\";
            TCPServer.SaveTo = recieve_path;
            TCPServer.Port = Convert.ToInt32(txt_target_port.Text);
            TCPServer obj_server = new TCPServer(this);
            System.Threading.Thread obj_thread = new
            System.Threading.Thread(obj_server.Startserver);
            obj_thread.Start();
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");  
        }

    }

}
