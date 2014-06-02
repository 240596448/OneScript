﻿using ScriptEngine.Compiler;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    class TextCompiler
    {
        CompilerContext _context;

        public TextCompiler(CompilerContext context)
        {
            _context = context;
        }

        public ModuleImage Load(string source)
        {
            return CreateModule(source);
        }

        private ModuleImage CreateModule(string source)
        {
            _context.PushScope(new SymbolScope());
            _context.DefineVariable("ЭтотОбъект");
            var parser = new Parser();
            parser.Code = source;

            var compiler = new Compiler.Compiler();
            ModuleImage compiledImage;
            try
            {
                compiledImage = compiler.Compile(parser, _context);
            }
            finally
            {
                _context.PopScope();
            }

            return compiledImage;
        }

    }

}
