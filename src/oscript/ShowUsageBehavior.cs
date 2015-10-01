﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class ShowUsageBehavior : AppBehavior
    {
        public override int Execute()
        {
            Console.WriteLine("1Script Execution Engine. Version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("I. Script execution: oscript.exe <script_path> [script arguments..]");
            Console.WriteLine();
            Console.WriteLine("II. Special mode: oscript.exe <mode> <script_path> [script arguments..]");
            Console.WriteLine("Mode can be one of these:");
            Console.WriteLine("  {0,-11}measures execution time", "-measure");
            Console.WriteLine("  {0,-11}shows compiled module without execution", "-compile");
            Console.WriteLine("  {0} set output encoding", "-encoding=<encoding-name>");
            Console.WriteLine();
            Console.WriteLine("III. Build standalone executable: oscript.exe -make <script_path> <output_exe>");
            Console.WriteLine("  Builds a standalone executable module based on script specified");
            Console.WriteLine();
            Console.WriteLine("IV. Run as CGI application: oscript.exe -cgi <script_path> [script arguments..]");
            Console.WriteLine("  Runs as CGI application under HTTP-server (Apache/Nginx/IIS/etc...)");

            return 0;
        }
    }
}
