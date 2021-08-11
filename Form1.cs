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
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;

using Monitel.Supervisor.Client;

namespace startMAGterminal
{
    public partial class Form1 : Form
    {
        string Monitel_CK11_Path= @"C:\Program Files\Monitel\ck11\client";
        string Monitel_PlatformInfrastructure_dll= "Monitel.PlatformInfrastructure.dll";
        string Monitel_Supervisor_Client_dll = "Monitel.Supervisor.Client.dll";
        string Monitel_Supervisor_Infrastructure_dll = "Monitel.Supervisor.Infrastructure.dll";
        const String regKey_Monitel = @"SOFTWARE\Monitel\CK-11\Installation\";
        const String regParam_ClientPath = "ClientPath";
        Color richTextBox1_SelectionColor;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)        {/*/MessageBox.Show("load","");        */
            richTextBox1_SelectionColor = richTextBox1.ForeColor;
        }

        private void button1_Click(object sender, EventArgs e)        {            Close();        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            #region Read registry
            richTextBox1.AppendText(@"Чтение реестра HKLM\"+ regKey_Monitel+ regParam_ClientPath);
            RegistryKey reg, regHKLM;            
            try
            {
                regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                reg = regHKLM.CreateSubKey(regKey_Monitel, true);
                Monitel_CK11_Path=reg.GetValue(regParam_ClientPath,"").ToString();
                if (String.IsNullOrEmpty(Monitel_CK11_Path)) throw new Exception("Не удалось получить информацию из реестра") ;
                richTextBox1.SelectionColor = Color.Lime;
                richTextBox1.AppendText(" OK");
                richTextBox1.SelectionColor = richTextBox1_SelectionColor;
                richTextBox1.AppendText(Monitel_CK11_Path);
            }
            catch (Exception ee)
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(" Ошибка");
                richTextBox1.SelectionColor = richTextBox1_SelectionColor;                
                richTextBox1.AppendText("\n" + ee.Message);
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, richTextBox1.Font.Style | FontStyle.Bold);
                richTextBox1.AppendText("\n Завершение работы через ");
            }
            #endregion
            //Assembly assembly =
            //Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_Supervisor_Client_dll));
            //Assembly assembly =
            //Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path,Monitel_Supervisor_Infrastructure_dll));
            //Assembly assembly =
            //Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_PlatformInfrastructure_dll));            
            //#region SuperVisor
            //var sv = new SupervisorClient();
            //while (true)
            //{
            //    try
            //    {
            //        if (!sv.IsConnected)
            //        {
            //            Console.WriteLine("Connect...");
            //            sv.Connect();
            //        }

            //        if (sv.IsLogged)
            //            break;

            //        Console.WriteLine("User not logged in");
            //        Thread.Sleep(3000);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //        Thread.Sleep(3000);
            //    }
            //}
            //sv.Dispose();
            //#endregion

        }
    }
}
