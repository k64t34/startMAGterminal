using System;

using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Monitel.Supervisor.Client;
using System.Threading;

namespace startMAGterminal
{
    public partial class Form1 : Form
    {
        string Monitel_CK11_Path= @"C:\Program Files\Monitel\ck11\client";
        string Monitel_PlatformInfrastructure_dll= "Monitel.PlatformInfrastructure.dll";
        string Monitel_Supervisor_Client_dll = "Monitel.Supervisor.Client.dll";
        string Monitel_Supervisor_Infrastructure_dll = "Monitel.Supervisor.Infrastructure.dll";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)        {/*/MessageBox.Show("load","");        */}

        private void button1_Click(object sender, EventArgs e)        {            Close();        }

        private void Form1_Shown(object sender, EventArgs e)
        {         
            //Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_Supervisor_Client_dll));
            //Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path,Monitel_Supervisor_Infrastructure_dll));
            //Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_PlatformInfrastructure_dll));            
            var sv = new SupervisorClient();
            while (true)
            {
                try
                {
                    if (!sv.IsConnected)
                    {
                        Console.WriteLine("Connect...");
                        sv.Connect();
                    }

                    if (sv.IsLogged)
                        break;

                    Console.WriteLine("User not logged in");
                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(3000);
                }
            }
            sv.Dispose();

        }
    }
}
