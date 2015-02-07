﻿using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("РежимСохраненияПутейZIP", "ZIPStorePathsMode")]
    public class ZipStorePathModeEnum : EnumerationContext
    {
        const string DONT_SAVE = "НеСохранять";
        const string SAVE_RELATIVE = "СохранятьОтносительныеПути";
        const string SAVE_FULL = "СохранятьПолныеПути";

        public ZipStorePathModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        [EnumValue(DONT_SAVE)]
        public EnumerationValue DontStorePath
        {
            get
            {
                return this[DONT_SAVE];
            }
        }

        [EnumValue(SAVE_RELATIVE)]
        public EnumerationValue StoreRelativePath
        {
            get
            {
                return this[SAVE_RELATIVE];
            }
        }

        [EnumValue(SAVE_FULL)]
        public EnumerationValue StoreFullPath
        {
            get
            {
                return this[SAVE_FULL];
            }
        }

        public static ZipStorePathModeEnum CreateInstance()
        {
            ZipStorePathModeEnum instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<ZipStorePathModeEnum>(out enumType, out enumValType);

            instance = new ZipStorePathModeEnum(enumType, enumValType);

            EnumContextHelper.RegisterValues<ZipStorePathModeEnum>(instance);

            return instance;
        }
    }
}
