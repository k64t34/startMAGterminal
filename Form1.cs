﻿using System;

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
using System.ServiceProcess;

using Monitel.Auth;
using Monitel.Supervisor.Client;

namespace startMAGterminal
{
    public partial class Form1 : Form
    {
        string Monitel_CK11_Path = @"C:\Program Files\Monitel\ck11\client";
        string exeMAGTerminal = "MagTerminal.exe";
        //string Monitel_PlatformInfrastructure_dll= "Monitel.PlatformInfrastructure.dll";
        //string Monitel_Supervisor_Client_dll = "Monitel.Supervisor.Client.dll";
        //string Monitel_Supervisor_Infrastructure_dll = "Monitel.Supervisor.Infrastructure.dll";
        const String regKey_Monitel = @"SOFTWARE\Monitel\CK-11\Installation\";
        const String regParam_ClientPath = "ClientPath";
        DateTime startTime= DateTime.Now, finishTime; // Collect time spend to run        
#if DEBUG
        int Timeout = 30;
#else
        int Timeout = 600;
#endif
        int TimeCounter = 0;
        int minTime , maxTime, avavrgTime;             // Statistic time spend previous launches 
        TimerCallback delegateTimerCallback;
        System.Threading.Timer AppTimer;
        Color richTextBox1_SelectionColor;
        Font richTextBox1_SelectionFont;
        FontStyle richTextBox1_SelectionFontStyle;
        int Status = 1; // 1 - Normal -waiting to start service; -1 - countdown - fail to start service and begin countdown before exit; 0 - MagTerminal alredy running

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {/*/MessageBox.Show("load","");        */            
            richTextBox1_SelectionColor = richTextBox1.ForeColor;
            richTextBox1_SelectionFont = richTextBox1.Font;
            richTextBox1_SelectionFontStyle = richTextBox1.Font.Style;
            delegateTimerCallback = new TimerCallback(TimerCallback);
            this.Height = 1080;
            this.Width = 1920;
            this.CenterToScreen(); //DONE: Центрировать форму после измеения размера            
            this.Text += " v " + Application.ProductVersion; //DONE: Версия программы //DONE: Версия MAGterminal                           
            label_TimeCounter.Left = 0;// (this.Width-label_TimeCounter.Width)/ 2;
            label_TimeCounter.BackColor = progressBar1.BackColor;            
            label_TimeCounter.Height=progressBar1.Height-2;
            label_TimeCounter.Top = progressBar1.Top+1; //TOTO: Прозрачный label в центра прогресс бара

            minTime = Convert.ToInt32( Math.Floor((float)Timeout / 2));
            maxTime = Timeout;
            avavrgTime = Convert.ToInt32(Math.Floor((float)(maxTime - minTime) / 2));
        }
        public void TimerCallback(object obj)
        {
            TimeCounter++;
            if (Status == 1)
            {
                Timeout--;
                if (Timeout == 0)
                {
                    ShowDownCount();
                }
                else
                {
                    /*if (TimeCounter>=minTime)
                        Invoke((MethodInvoker)(() =>
                        {
                            progressBar1.BackColor=Color.Yellow;
                        }));*/
                    Invoke((MethodInvoker)(() =>
                    {
                        label_TimeCounter.Text= TimeCounter.ToString();
                    }));

                    if (progressBar1.Value + 1 <= progressBar1.Maximum)
                        Invoke((MethodInvoker)(() =>
                        {
                            progressBar1.Value++;
                        }));
                }
            }
            else if (Status == -1)
            {
                Timeout--;
                if (progressBar1.Value > 0)
                    Invoke((MethodInvoker)(() =>
                    {
                        progressBar1.Value--;
                    }));
                if (Timeout <= 0)
                {
                    StopTimerAndExit();
                }                   
            }
        }
        private void button1_Click(object sender, EventArgs e) { StopTimerAndExit(); }
        private void Form1_Shown(object sender, EventArgs e)
        {
            progressBar1.Maximum = Timeout;
            progressBar1.Minimum = 0;
            AppTimer = new System.Threading.Timer(delegateTimerCallback, null, 0, 1000);
            Loop();
        }
        async void Loop()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                //DONE: Проверка на самозапуск                
                Process[] SelfProc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
                if (SelfProc.Length > 1) Application.Exit();
                //DONE: Проверка что процесс еще не запусщен                
                Process[] MAGTerminalProc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeMAGTerminal));                
                if (MAGTerminalProc.Length > 0)
                {
                    logWARN("Процесс "+ exeMAGTerminal+" уже запущен");
                    Status = 0;
                    ShowDownCount();
                    return;
                }                
                //TODO: Статистика времени запуска: min, max, average               


                //TODO: Проверка сетевых служб
                FontFamily familyArial = new FontFamily("Arial");
                Font fontArialBold24 = new Font(familyArial, 24.0f, FontStyle.Bold);

                Thread.Sleep(1000);
                log(TimeCounterStr()+"Контроллер видеостены ");
                log(Environment.GetEnvironmentVariable("COMPUTERNAME").ToUpper()+"\n", fontArialBold24);                //DONE: Отображение имени контроллера видеостены

                Thread.Sleep(1000);
                log(TimeCounterStr() + "Пользователь ");
                log(Environment.GetEnvironmentVariable("USERDOMAIN")+"\\"+ Environment.GetEnvironmentVariable("USERNAME") + "\n", fontArialBold24);//DONE: Отображение имени контроллера видеостены
                #region Get CK11 path from registry
                Thread.Sleep(1000);
                log(TimeCounterStr() + @"Чтение реестра HKLM\" + regKey_Monitel + regParam_ClientPath);
                RegistryKey reg, regHKLM;
                try
                {
                    regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    reg = regHKLM.CreateSubKey(regKey_Monitel, true);
                    Monitel_CK11_Path = reg.GetValue(regParam_ClientPath, "").ToString();
                    if (String.IsNullOrEmpty(Monitel_CK11_Path)) throw new Exception("Не удалось получить информацию из реестра");
                    logOK();                     ////richTextBox1.SelectionFont = new Font(richTextBox1.Font, richTextBox1.Font.Style | FontStyle.Bold);
                    log("\t\t"+Monitel_CK11_Path + "\n");
                }
                catch (Exception e)
                {
                    logWARN(" "+e.Message);
                    Monitel_CK11_Path = Path.GetDirectoryName(Application.ExecutablePath); //System.IO.Path.GetDirectoryName(filename)                    Path.GetDirectoryName(filename);                    Path.GetFullPath(fileName);                    Path.GetFullPath(filePath)
                    log("\t\t" + Monitel_CK11_Path + "\n");
                }
                #endregion
                #region Check file MAGTerminal
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Поиск файла " + exeMAGTerminal+" в папке "+ Monitel_CK11_Path);
                if (File.Exists(Path.Combine(Monitel_CK11_Path, exeMAGTerminal))) logOK();
                else
                {
                    logERROR("Файл не найден"); ShowDownCount(); return;
                }

                #endregion
                #region CK-11 Supervisor                
                //string svc_description = "Служба управления задачами СК-11 CK11",
                string scv_name = "CK11SupervisorSvc",
                svc_title = "CK-11 Supervisor";
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Ожидание запуска службы " + svc_title);
                try
                {
                    ServiceController sc = new ServiceController(scv_name);
                    while (Status == 1)
                    {
                        if (sc.Status == ServiceControllerStatus.Running) { logOK(); break; }
                        Thread.Sleep(1000);
                        log(".");
                        sc.Refresh();
                    }
                    if (Status == -1) { logERROR("Служба не запущена"); return; }
                }
                catch (Exception e) { logERROR(e.Message); ShowDownCount(); return; }

                #endregion
                #region CK-11 Autoupdate service               
                //svc_description = "Служба обновления клиентских модулей СК-11";
                scv_name = "CK11AutoUpdService";
                svc_title = "CK-11 Autoupdate service";
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Ожидание запуска службы " + svc_title);
                try
                {
                    ServiceController sc = new ServiceController(scv_name);
                    if (sc.Status == ServiceControllerStatus.Running) { logOK(); }
                    else logWARN(" Служба не запущена");
                }
                catch (Exception e)
                {
                    logWARN(" " + e.Message);
                }
                #endregion
                #region CK-11 LogWriter               
                //svc_description = "Служба журналирования СК-11";
                scv_name = "CK11LogWriterService";
                svc_title = "CK-11 LogWriter";
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Ожидание запуска службы " + svc_title);
                try
                {
                    ServiceController sc = new ServiceController(scv_name);
                    if (sc.Status == ServiceControllerStatus.Running) { logOK(); }
                    else logWARN(" Служба не запущена");
                }
                catch (Exception e)
                {
                    logWARN(" " + e.Message);
                }
                #endregion
                #region Auth Monitel User
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Проверка авторизации в CK11 ");
                try
                {
                    AuthContext.Init(null);
                    logOK();
                }
                catch (Exception ex)
                {
                    logERROR(ex.Message);                    
                }
                #endregion
                #region UserLogin          
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Ожидание подключения к сервису CK11");
                Monitel.Supervisor.Client.SupervisorClient sv;
                sv = new SupervisorClient();
                while (Status == 1)
                {
                    Thread.Sleep(1000);
                    try
                    {                       
                        if (sv != null)
                        {
                            if (!sv.IsConnected)
                            {
                                sv.Connect(null);
                            }
                            if (sv.IsLogged)
                            {
                                logOK();
                                break;
                            }                            
                        }
                        log(".");
                    }
                    catch (Exception e)
                    {
                        log("_("+e.Message+")_,");
                    }                    
                }
                sv.Dispose();
                if (Status != 1)
                {
                    log("\n");
                    return;
                }
                #endregion
                #region Run process MAG Terminal
                Thread.Sleep(1000);
                log(TimeCounterStr() + "Ожидание запуска процесса MAGTerminal.exe");
                //DONE: Проверка что процесс еще не запусщен
                Process[] MAGTerminalProc2 = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeMAGTerminal));
                if (MAGTerminalProc2.Length > 0)
                {
                    logWARN("Процесс " + exeMAGTerminal + " уже запущен");
                    ShowDownCount();
                }
                try
                {
                    //TODO: Проверка что процесс успешно запустился
                #if ! DEBUG

                    Process.Start(Path.Combine(Monitel_CK11_Path, exeMAGTerminal));
                    Invoke((MethodInvoker)(() =>
                    {
                        richTextBox1.BackColor = Color.DarkGreen;
                    }));
                #endif 
                    logOK();
                    StopTimerAndExit();

                }
                catch (Exception e) { logERROR(e.Message); ShowDownCount(); }
                return;
#endregion
            });
        }
        void ShowDownCount()
        {
            if (Status == -1) return;
            if (Status != 0)
            {
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.BackColor = Color.DarkRed;
                }));
                FontFamily family = new FontFamily("Arial");
                Font font = new Font(family, 16.0f, FontStyle.Bold);
                log("\n\n\nНЕ УДАЛОСЬ ЗАПУСТИТЬ MAG Terminal\n", font);//new Font("Segoe UI", 9, FontStyle.Bold);
            }
            Status = -1;
#if DEBUG
            Timeout = 10;
#else
        Timeout = 60;
#endif
            Invoke((MethodInvoker)(() =>
            {
                progressBar1.Maximum = Timeout;
                progressBar1.Value = Timeout;
            }));         
            log(String.Format("\n\n Завершение работы программы через {0} сек",Timeout));
        }
        void StopTimerAndExit()
        {
            AppTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            Thread.Sleep(5000);
            Application.Exit();
        }
        void log(String text)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke((MethodInvoker)(() =>
                { richTextBox1.AppendText(text); richTextBox1.ScrollToCaret(); }));
            }
            else
            {
                richTextBox1.AppendText(text); richTextBox1.ScrollToCaret();
            }
        }
        
        void log(String text,Color c)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.SelectionColor = c;
                    richTextBox1.AppendText(text);
                    richTextBox1.SelectionColor = richTextBox1_SelectionColor;
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                    richTextBox1.SelectionColor = c;
                    richTextBox1.AppendText(text);
                    richTextBox1.SelectionColor = richTextBox1_SelectionColor;
                    richTextBox1.ScrollToCaret();
            }

        }
        void log(String text, Color c, FontStyle s)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.SelectionColor = c;
                    richTextBox1.SelectionFont = new Font(richTextBox1_SelectionFont, s);
                    richTextBox1.AppendText(text);
                    richTextBox1.SelectionColor = richTextBox1_SelectionColor;
                    richTextBox1.SelectionFont = richTextBox1_SelectionFont;
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.SelectionColor = c;
                richTextBox1.SelectionFont = new Font(richTextBox1_SelectionFont, s);
                richTextBox1.AppendText(text);
                richTextBox1.SelectionColor = richTextBox1_SelectionColor;
                richTextBox1.SelectionFont = richTextBox1_SelectionFont;
                richTextBox1.ScrollToCaret();
            }

        }
        void log(String text, Font f)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.SelectionFont = f ;
                    richTextBox1.AppendText(text);
                    richTextBox1.SelectionFont = richTextBox1_SelectionFont;
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.SelectionFont = f;
                richTextBox1.AppendText(text);
                richTextBox1.SelectionFont = richTextBox1_SelectionFont;
                richTextBox1.ScrollToCaret();
            }

        }
        void logOK() { log(" OK\n", Color.Lime, FontStyle.Bold); }
        void logWARN(string text) { log(text+"\n", Color.Orange, FontStyle.Bold); }
        void logERROR(string text) {
            log(" ОШИБКА\n", Color.Red, FontStyle.Bold);
            log(text + "\n"); 
        }
        String TimeCounterStr() { return String.Format("{0,3} ",TimeCounter); } //TODO:Оптимизировать эту функцию


    }
}
