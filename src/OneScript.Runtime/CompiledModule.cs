﻿using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    class CompiledModule : ILoadedModule
    {
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ISourceCodeIndexer SourceCodeIndexer
        {
            get { throw new NotImplementedException(); }
        }
    }
}
