﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class CmdLineHelper
    {
        private string[] _args;
        private int _index = 0;

        public CmdLineHelper(string[] args)
        {
            _args = args;
        }

        public string Next()
        {
            if (_index == _args.Length)
                return null;

            return _args[_index++];
        }

        public string Current()
        {
            if (_index < 0 || _index >= _args.Length)
                return null;

            return _args[_index];
        }

        public string[] Tail()
        {
            return _args.Skip(_index).ToArray();
        }
    }
}
