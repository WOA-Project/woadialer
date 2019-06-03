using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;

namespace WoADialerHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                startListeningMode();
            }
            else
            {
                Console.WriteLine("Running parameters mode...");
                for (int i = 0; i < args.Length; i++) Console.WriteLine("Arg " + i + " = " + args[i]);
                string command = args[0].Split(':')[1];
                if (command == "closecall")
                {
                    ServiceController controller = new ServiceController("PhoneSvc");
                    try
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                        controller.Start();
                    }
                    catch (Exception e) { }
                }
            }
        }

        public static void startListeningMode()
        {
            PhoneCallManager.CallStateChanged += (o, args) =>
            {
                Console.WriteLine("Call state changed!");
                if (PhoneCallManager.IsCallIncoming || PhoneCallManager.IsCallActive)
                {
                    Console.WriteLine("It's a call incoming!");
                    Console.WriteLine("Starting WoADialer...");
                    try
                    {
                        Process woadialer = Process.Start("WoADialer.exe");
                        if (woadialer == null)
                        {
                            Console.WriteLine("The process did not start or the console wasn't able to catch it.");
                            Console.WriteLine("Gotta catch 'em all!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to launch WoADialer. Exception details:");
                        Console.WriteLine(e.StackTrace);
                    }
                    Console.WriteLine("WoADialer launched.");
                }
            };
            Console.WriteLine("Call event bound!");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
