﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class Program
    {
        static int Main(string[] args)
        {
            int returnCode;
            var behavior = BehaviorSelector.Select(args);
            try
            {
                returnCode = behavior.Execute();
            }
            catch (Exception e)
            {
                // сюда при выполнении скрипта мы попадать не должны
                // исключения времени выполнения выводятся в IApplicationHost.ShowExceptionInfo
                Console.WriteLine(e.ToString());
                returnCode = 1;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Write("Press any key to continue. . . ");
                Console.ReadKey(true);
            }

            return returnCode;

        }
    }
}
