﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    public static class FileOpener
    {

        public static StreamReader OpenReader(string filename)
        {
            FileStream input = new FileStream(filename, FileMode.Open);
            Encoding enc = AssumeEncoding(input);

            return new StreamReader(input, enc, true);

        }

        public static StreamReader OpenReader(string filename, Encoding encoding)
        {
            return new StreamReader(filename, encoding);
        }

        public static StreamWriter OpenWriter(string filename)
        {
            return new StreamWriter(filename, false, Encoding.UTF8);
        }

        public static StreamWriter OpenWriter(string filename, Encoding encoding)
        {
            return new StreamWriter(filename, false, encoding);
        }

        public static StreamWriter OpenWriter(string filename, Encoding encoding, bool append)
        {
            return new StreamWriter(filename, append, encoding);
        }

        public static Encoding AssumeEncoding(Stream inputStream)
        {
            Encoding enc;
            // *** Use Default of Encoding.Default (Ansi CodePage)
            enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];

            inputStream.Read(buffer, 0, 5);
            inputStream.Position = 0;

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;

            return enc;
        }
    }
}
