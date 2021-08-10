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
            //Кандидат на решение https://professorweb.ru/my/csharp/assembly/level1/1_7.php
            Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_Supervisor_Client_dll));
            //Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path,Monitel_Supervisor_Infrastructure_dll));
            //Assembly assembly = Assembly.LoadFrom(Path.Combine(Monitel_CK11_Path, Monitel_PlatformInfrastructure_dll));
            //Перебор типов в dll https://www.chilkatsoft.com/p/p_502.asp

            // получаем все типы из сборки .dll //https://metanit.com/sharp/tutorial/14.2.php
            Type[] types = assembly.GetTypes();
            foreach (Type t in types)
            {
                //listBox1.Items.Add(t.Name);
                if (t.Name.CompareTo("SupervisorClient") == 0) 
                { 
                    foreach (MemberInfo mi in t.GetMembers())
                    {
                        listBox1.Items.Add($"\t{mi.DeclaringType} {mi.MemberType} {mi.Name}");                
                    }
                    foreach (MethodInfo method in t.GetMethods())
                    {
                        string modificator = "";
                        if (method.IsStatic)
                            modificator += "static ";
                        if (method.IsVirtual)
                            modificator += "virtual";
                        listBox1.Items.Add($"\t{modificator} {method.ReturnType.Name} {method.Name} (");
                        //получаем все параметры
                        ParameterInfo[] parameters = method.GetParameters();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            listBox1.Items.Add($"\t\t{parameters[i].ParameterType.Name} {parameters[i].Name}");
                            if (i + 1 < parameters.Length) Console.Write(", ");
                        }
                    }
                    listBox1.Items.Add("\tКонструкторы");
                    foreach (ConstructorInfo ctor in t.GetConstructors())
                    {
                        listBox1.Items.Add("\t\t"+t.Name + " (");
                        // получаем параметры конструктора
                        ParameterInfo[] parameters = ctor.GetParameters();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            listBox1.Items.Add("\t\t\t"+parameters[i].ParameterType.Name + " " + parameters[i].Name);
                            if (i + 1 < parameters.Length) Console.Write(", ");
                        }                        
                    }
                }
            }


            //Type type = assembly.GetType("Monitel.Supervisor.Client.SupervisorClient");
            //if (type == null) { listBox1.Items.Add("type  SupervisorClient not found"); return; }
            //var sv = Activator.CreateInstance(type/*, new object[]{}*/);
            //if (sv == null) throw new Exception("broke");
            //MethodInfo method = type.GetMethod("IsConnected");
            //object result = method.Invoke(sv, new object[] { }); //Возможно SupervisorClient  не класс а интерфейс https://coderoad.ru/18959370/C-%D0%B7%D0%B0%D0%B3%D1%80%D1%83%D0%B7%D0%BA%D0%B0-%D0%B8%D0%BD%D1%82%D0%B5%D1%80%D1%84%D0%B5%D0%B9%D1%81%D0%B0-%D0%BD%D0%B5-%D0%BA%D0%BB%D0%B0%D1%81%D1%81%D0%B0-%D0%B2%D0%BE-%D0%B2%D1%80%D0%B5%D0%BC%D1%8F-%D0%B2%D1%8B%D0%BF%D0%BE%D0%BB%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F-%D0%B8%D0%B7-DLL

            //MessageBox.Show("shown", "");
            //var sv = new SupervisorClient();
            /*while (true)
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
            sv.Dispose();*/

        }
    }
}
