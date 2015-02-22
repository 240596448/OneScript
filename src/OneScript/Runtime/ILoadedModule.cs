﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Compiler;

namespace OneScript.Runtime
{
    public interface ILoadedModule
    {
        string Name { get; set; }
        ISourceCodeIndexer SourceCodeIndexer { get; }
    }
}
