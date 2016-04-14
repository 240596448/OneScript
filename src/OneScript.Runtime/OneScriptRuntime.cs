﻿using OneScript.Language;
using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Runtime.Compiler;

namespace OneScript.Runtime
{
    public class OneScriptRuntime : IScriptRuntime
    {
        private RuntimeValuesHolder _externalProperties = new RuntimeValuesHolder();
        private List<RuntimeScope> _externalContexts = new List<RuntimeScope>();

        private CompilerContext _ctx = new CompilerContext();
        private TypeManager _typeManager;

        public OneScriptRuntime()
        {
            _typeManager = new TypeManager();
            PreprocessorDirectives = new PreprocessorDirectivesSet();
            _ctx.PushScope(_externalProperties);
        }

        public void InjectSymbol(string name, IValue value)
        {
            _externalProperties.DefineVariable(name, value);
        }

        public void InjectObject(IRuntimeContextInstance context)
        {
            var scope = RuntimeScope.FromContext(context);

            CheckVariablesConflicts(scope);
            CheckMethodsConflicts(scope);

            _ctx.PushScope(scope);
            _externalContexts.Add(scope);
        }

        public void InjectVariable(InjectedVariable variable)
        {
            InjectSymbol(variable.Name, variable);
        }

        private void CheckVariablesConflicts(ISymbolScope scope)
        {
            if (scope.VariableCount == 0)
                return;

            foreach (var name in scope.GetVariableSymbols())
            {
                if (_ctx.IsVarDefined(name))
                    throw new ArgumentException("Переменная (" + name + ") уже определена");
            }
        }

        private void CheckMethodsConflicts(ISymbolScope scope)
        {
            if (scope.MethodCount == 0)
                return;

            foreach (var name in scope.GetMethodSymbols())
            {
                if (_ctx.IsMethodDefined(name))
                    throw new ArgumentException("Метод (" + name + ") уже определен");
            }
        }

        public DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null)
        {
            return _typeManager.RegisterType(name, alias, constructor);
        }

        public IValue Eval(string expression)
        {
            throw new NotImplementedException();
        }

        public ICompiledModule Compile(IScriptSource moduleSource)
        {
            var compiler = GetCompilerService();
            return compiler.CompileModule(moduleSource);
        }

        public CompilerService GetCompilerService()
        {
            var parserClient = new OSByteCodeBuilder();
            parserClient.Context = _ctx;
            var parser = new Parser(parserClient);
            var pp = new Preprocessor();
            
            foreach (var item in PreprocessorDirectives)
            {
                pp.Define(item);
            }

            return new CompilerService(null, pp, parserClient);
        }

        public void Execute(ICompiledModule module, string entryPointName)
        {
            var process = CreateProcess();

            process.Execute(module, entryPointName);

        }

        public OneScriptProcess CreateProcess()
        {
            var process = new OneScriptProcess(this);

            process.Memory.AddScope(_externalProperties);
            foreach (var ctx in _externalContexts)
            {
                process.Memory.AddScope(ctx);
            }
            return process;
        }

        public PreprocessorDirectivesSet PreprocessorDirectives { get; private set; }

        internal TypeManager TypeManager
        {
            get { return _typeManager; }
        }

    }
}
