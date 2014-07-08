﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    class CompilerContext : ICompilerContext
    {
        List<SymbolScope> _scopeStack = new List<SymbolScope>();

        public void PushScope(SymbolScope scope)
        {
            _scopeStack.Add(scope);
        }

        public SymbolScope PopScope()
        {
            var idx = _scopeStack.Count - 1;
            if (idx >= 0)
            {
                var retVal = _scopeStack[idx];
                _scopeStack.RemoveAt(idx);
                return retVal;
            }
            else
            {
                throw new InvalidOperationException("No scopes defined");
            }
        }

        public SymbolScope Peek()
        {
            var idx = _scopeStack.Count - 1;
            if (idx >= 0)
            {
                return _scopeStack[idx];
            }
            else
            {
                throw new InvalidOperationException("No scopes defined");
            }
        }

        private SymbolBinding GetSymbol(string symbol, Func<SymbolScope, int> extract)
        {
            for (int i = _scopeStack.Count - 1; i >= 0; i--)
            {
                try
                {
                    var number = extract(_scopeStack[i]);
                    var result = new SymbolBinding();
                    result.CodeIndex = number;
                    result.ContextIndex = i;
                    return result;
                }
                catch (SymbolNotFoundException)
                {
                    continue;
                }
            }

            throw new SymbolNotFoundException(symbol);
        }

        private bool HasSymbol(Func<SymbolScope, bool> definitionCheck)
        {
            for (int i = _scopeStack.Count - 1; i >= 0; i--)
            {
                var isDefined = definitionCheck(_scopeStack[i]);
                if (isDefined)
                    return true;
            }

            return false;
        }

        public VariableBinding GetVariable(string name)
        {
            var sb = GetSymbol(name, x=>x.GetVariableNumber(name));

            return new VariableBinding()
            {
                type = _scopeStack[sb.ContextIndex].GetVariable(sb.CodeIndex).Type,
                binding = sb
            };

        }

        public SymbolBinding GetMethod(string name)
        {
            return GetSymbol(name, x => x.GetMethodNumber(name));
        }

        public SymbolScope GetScope(int scopeIndex)
        {
            return _scopeStack[scopeIndex];
        }

        public int ScopeIndex(SymbolScope scope)
        {
            return _scopeStack.IndexOf(scope);
        }

        public SymbolBinding DefineMethod(MethodInfo method)
        {
            if (_scopeStack.Count > 0)
            {
                if (!HasSymbol(x => x.IsMethodDefined(method.Name)))
                {
                    var idx = TopIndex();
                    var num = _scopeStack[TopIndex()].DefineMethod(method);
                    return new SymbolBinding()
                    {
                        ContextIndex = idx,
                        CodeIndex = num
                    };
                }
                else
                    throw new CompilerException("Symbol already defined");
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public SymbolBinding DefineVariable(string name)
        {
            if (_scopeStack.Count > 0)
            {
                if (!HasSymbol(x => x.IsVarDefined(name)))
                {
                    var idx = TopIndex();
                    var num = _scopeStack[idx].DefineVariable(name);
                    return new SymbolBinding()
                    {
                        ContextIndex = idx,
                        CodeIndex = num
                    };
                }
                else
                    throw new CompilerException("Symbol already defined");
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public SymbolBinding DefineProperty(string name)
        {
            if (_scopeStack.Count > 0)
            {
                var idx = TopIndex();
                var num = _scopeStack[idx].DefineVariable(name, SymbolType.ContextProperty);
                return new SymbolBinding()
                {
                    ContextIndex = idx,
                    CodeIndex = num
                };
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public int TopIndex()
        {
            return _scopeStack.Count - 1;
        }

    }
}
