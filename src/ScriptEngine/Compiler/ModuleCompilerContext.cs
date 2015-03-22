﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    class ModuleCompilerContext : ICompilerContext
    {
        private CompilerContext _outerCtx;
        private CompilerContext _moduleCtx;
        private int OUTER_CTX_SIZE;
        private int _localScopesCount = 0;

        public ModuleCompilerContext(CompilerContext outerContext)
        {
            _outerCtx = outerContext;
            _moduleCtx = new CompilerContext();
            Rebase();
        }
        
        public void Rebase()
        {
            OUTER_CTX_SIZE = _outerCtx.TopIndex() + 1;
        }

        #region ICompilerContext Members

        public SymbolBinding DefineMethod(MethodInfo method)
        {
            var sb = _moduleCtx.DefineMethod(method);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding DefineProperty(string name)
        {
            var sb = _moduleCtx.DefineProperty(name);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding DefineVariable(string name)
        {
            var sb = _moduleCtx.DefineVariable(name);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding GetMethod(string name)
        {
            try
            {
                var sb = _moduleCtx.GetMethod(name);
                ShiftIndex(ref sb);

                return sb;
            }
            catch (SymbolNotFoundException)
            {
                return _outerCtx.GetMethod(name);
            }
        }

        public SymbolScope GetScope(int scopeIndex)
        {
            if (scopeIndex < OUTER_CTX_SIZE)
            {
                return _outerCtx.GetScope(scopeIndex);
            }
            else
            {
                return _moduleCtx.GetScope(scopeIndex - OUTER_CTX_SIZE);
            }
        }

        public VariableBinding GetVariable(string name)
        {
            try
            {
                var vb = _moduleCtx.GetVariable(name);
                ShiftIndex(ref vb.binding);

                return vb;
                
            }
            catch (SymbolNotFoundException)
            {
                return _outerCtx.GetVariable(name);
            }
        }

        public SymbolScope Peek()
        {
            if (_localScopesCount > 0)
                return _moduleCtx.Peek();
            else
                return _outerCtx.Peek();
        }

        public SymbolScope PopScope()
        {
            var scope = _moduleCtx.PopScope();
            _localScopesCount--;

            return scope;

        }

        public void PushScope(SymbolScope scope)
        {
            _moduleCtx.PushScope(scope);
            _localScopesCount++;
        }

        public int ScopeIndex(SymbolScope scope)
        {
            int idx = _moduleCtx.ScopeIndex(scope);
            if (idx >= 0)
            {
                return idx + OUTER_CTX_SIZE;
            }
            else
            {
                idx = _outerCtx.ScopeIndex(scope);
            }

            return idx;
        }

        public int TopIndex()
        {
            if (_localScopesCount > 0)
            {
                return _moduleCtx.TopIndex() + OUTER_CTX_SIZE;
            }
            else
            {
                return _outerCtx.TopIndex();
            }
        }

        #endregion

        private void ShiftIndex(ref SymbolBinding symbolBinding)
        {
            symbolBinding.ContextIndex += OUTER_CTX_SIZE;
        }
    }
}
