﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Compiler
{
    public struct SymbolBinding
    {
        public string Name;
        public int Context;
        public int IndexInContext;
    }
}
