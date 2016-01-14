﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    static class Output
    {
        public static Action<string> Write { get; private set; }
        
        public static void Init()
        {
            if (Program.ConsoleOutputEncoding == null)
                Write = WriteStandardConsole;
            else
                Write = WriteEncodedStream;
        }

        public static void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        public static void WriteLine()
        {
            Write(Environment.NewLine);
        }

        private static void WriteStandardConsole(string text)
        {
            Console.Write(text);
        }

        private static void WriteEncodedStream(string text)
        {
            using (var stdout = Console.OpenStandardOutput())
            {
                var enc = Program.ConsoleOutputEncoding;
                var bytes = enc.GetBytes(text);
                stdout.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
