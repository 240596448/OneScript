﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public interface ILexemExtractor
    {
        Lexem LastExtractedLexem { get; }
        void NextLexem();
    }
}
