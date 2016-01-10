﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine
    {
        ScriptingEngine _engine;
        SystemGlobalContext _globalCtx;
        RuntimeEnvironment _env;
        bool _isInitialized;

        public HostedScriptEngine()
        {
            _engine = new ScriptingEngine();
            _env = new RuntimeEnvironment();
            _engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly(), _env);

            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx, false);
            _engine.Environment = _env;

        }

        public void InitExternalLibraries(string systemLibrary, IEnumerable<string> searchDirs)
        {
            var libLoader = new LibraryResolver(_engine, _env);
            _engine.DirectiveResolver = libLoader;

            libLoader.LibraryRoot = systemLibrary;
            libLoader.SearchDirectories.Clear();
            if (searchDirs != null)
            {
                libLoader.SearchDirectories.AddRange(searchDirs);
            }
        }

        public static string ConfigFileName
        {
            get
            {
                return EngineConfigProvider.CONFIG_FILE_NAME;
            }
        }

        public string CustomConfig { get; set; }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                var cfgAccessor = GlobalsManager.GetGlobalContext<SystemConfigAccessor>();
                cfgAccessor.Provider = new EngineConfigProvider(CustomConfig);
                cfgAccessor.Refresh();

                InitLibraries(cfgAccessor.GetConfig());

                _engine.Initialize();
                TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
                _isInitialized = true;
            }
        }

        private void InitLibraries(KeyValueConfig config)
        {
            if(config != null)
            {
                InitLibrariesFromConfig(config);
            }
            else
            {
                InitExternalLibraries(null, null);
            }
        }

        private static string ConfigFilePath()
        {
            return EngineConfigProvider.DefaultConfigFilePath();
        }

        private void InitLibrariesFromConfig(KeyValueConfig config)
        {
            string sysDir = config[EngineConfigProvider.SYSTEM_LIB_KEY];
            string additionalDirsList = config[EngineConfigProvider.ADDITIONAL_LIB_KEY];
            string[] addDirs = null;
            
            if(additionalDirsList != null)
            {
                addDirs = additionalDirsList.Split(';');
            }

            InitExternalLibraries(sysDir, addDirs);

        }

        public void AttachAssembly(System.Reflection.Assembly asm)
        {
            _engine.AttachAssembly(asm, _env);
        }

        public void InjectGlobalProperty(string name, IValue value, bool readOnly)
        {
            _env.InjectGlobalProperty(value, name, readOnly);
        }

        public void InjectObject(IAttachableContext obj, bool asDynamicScope)
        {
            _env.InjectObject(obj, asDynamicScope);
        }

        public ICodeSourceFactory Loader
        {
            get
            {
                return _engine.Loader;
            }
        }

        public CompilerService GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.DefineVariable("ЭтотОбъект", SymbolType.ContextProperty);
            return compilerSvc;
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src)
        {
            return CreateProcess(host, src, GetCompilerService());
        }

        public Process CreateProcess(IHostApplication host, ICodeSource src, CompilerService compilerSvc)
        {
            return CreateProcess(host, compilerSvc.CreateModule(src), src);
        }

        public Process CreateProcess(IHostApplication host, ScriptModuleHandle moduleHandle, ICodeSource src)
        {
            SetGlobalEnvironment(host, src);
            var module = _engine.LoadModuleImage(moduleHandle);
            return InitProcess(host, ref module);
        }

        private void SetGlobalEnvironment(IHostApplication host, ICodeSource src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        private Process InitProcess(IHostApplication host, ref LoadedModuleHandle module)
        {
            Initialize();
            var process = new Process(host, module, _engine);
            return process;
        }

    }
}
