﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;

namespace OneScript.Runtime
{
    interface IRuntimeContext
    {

    }

    public struct NamedValue
    {
        public string Name;
        public IValue Value;
    }
}
