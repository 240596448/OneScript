﻿using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptProcess : IScriptProcess
    {
        public OneScriptProcess(OneScriptRuntime world)
        {
            World = world;
            Memory = new MachineMemory();
        }

        private OneScriptRuntime World { get; set; }

        public TypeManager TypeManager
        {
            get
            {
                return World.TypeManager;
            }
        }

        internal void Execute(ICompiledModule module, string entryPointName)
        {
            throw new NotImplementedException();
        }

        public IRuntimeDataContext RuntimeContext
        {
            get { throw new NotImplementedException(); }
        }

        public MachineMemory Memory
        {
            get;
            private set;
        }

    }
}
