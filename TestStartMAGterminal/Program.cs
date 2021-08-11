using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Monitel.Supervisor.Client;

namespace TestStartMAGterminal
{
    class Program
    {
        static void Main(string[] args)
        {
            SupervisorClient sv;
            try
            {
                sv = new SupervisorClient();
                while (true)
                {
                    try
                    {
                        sv = new SupervisorClient();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); };
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
                    finally { sv.Dispose(); }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }            
        }
    }
}
