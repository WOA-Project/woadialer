using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WoADialerHelper2
{
    class Program
    {
        static void Main(string[] args)
        {



            if (args.Length == 0)
            {
                Console.WriteLine("Setting up the WoADialer Helper...");
                Registry.SetValue("HKEY_CLASSES_ROOT\\woadialerhelper", "", "URL:WoADialerHelper Protocol");
                Registry.SetValue("HKEY_CLASSES_ROOT\\woadialerhelper", "URL Protocol", "");

                Registry.SetValue("HKEY_CLASSES_ROOT\\woadialerhelper\\DefaultIcon", "", "WoADialerHelper2.exe,1");
                Registry.SetValue("HKEY_CLASSES_ROOT\\woadialerhelper\\shell\\open\\command", "", "\"C:\\WoADialerHelper2.exe\" \"%1\"");

                Console.WriteLine("Completed, press any key to close...");
                Console.ReadKey();
                return;
            }
            else
            {
                Console.WriteLine("Running parameters mode...");
                for(int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine("Arg " + i + " = " + args[i]);
                }
                string command = args[0].Split(':')[1];
                if(command == "closecall")
                {
                    ServiceController controller = new ServiceController("PhoneSvc");
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    controller.Start();
                }
            }
        }
    }
}
