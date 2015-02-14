﻿using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [SystemEnum("КодировкаТекста", "TextEncoding")]
    public class TextEncodingEnum : EnumerationContext
    {
        private const string ENCODING_ANSI = "ANSI";
        private const string ENCODING_OEM = "OEM";
        private const string ENCODING_UTF16 = "UTF16";
        private const string ENCODING_UTF8 = "UTF8";
        private const string ENCODING_SYSTEM = "Системная";

        private TextEncodingEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(ENCODING_ANSI)]
        public EnumerationValue Ansi
        {
            get
            {
                return this[ENCODING_ANSI];
            }
        }

        [EnumValue(ENCODING_OEM)]
        public EnumerationValue Oem
        {
            get
            {
                return this[ENCODING_OEM];
            }
        }

        [EnumValue(ENCODING_UTF16)]
        public EnumerationValue Utf16
        {
            get
            {
                return this[ENCODING_UTF16];
            }
        }

        [EnumValue(ENCODING_UTF8)]
        public EnumerationValue Utf8
        {
            get
            {
                return this[ENCODING_UTF8];
            }
        }

        [EnumValue(ENCODING_SYSTEM)]
        public EnumerationValue System
        {
            get
            {
                return this[ENCODING_SYSTEM];
            }
        }

        public static TextEncodingEnum CreateInstance()
        {
            TextEncodingEnum instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<TextEncodingEnum>(out enumType, out enumValType);

            instance = new TextEncodingEnum(enumType, enumValType);

            EnumContextHelper.RegisterValues<TextEncodingEnum>(instance);

            return instance;
        }

        public static Encoding GetEncoding(IValue encoding)
        {
            if (encoding.DataType == DataType.String)
                return Encoding.GetEncoding(encoding.AsString());
            else
            {
                if (encoding.DataType != DataType.GenericValue)
                    throw RuntimeException.InvalidArgumentType();

                var encValue = encoding.GetRawValue() as SelfAwareEnumValue<TextEncodingEnum>;
                if (encValue == null)
                    throw RuntimeException.InvalidArgumentType();

                var encodingEnum = GlobalsManager.GetEnum<TextEncodingEnum>();

                Encoding enc;
                if (encValue == encodingEnum.Ansi)
                    enc = Encoding.GetEncoding(1251);
                else if (encValue == encodingEnum.Oem)
                    enc = Encoding.GetEncoding(866);
                else if (encValue == encodingEnum.Utf16)
                    enc = new UnicodeEncoding(false, true);
                else if (encValue == encodingEnum.Utf8)
                    enc = new UTF8Encoding(true);
                else if (encValue == encodingEnum.System)
                    enc = Encoding.Default;
                else
                    throw RuntimeException.InvalidArgumentValue();

                return enc;
            }
        }

    }
}
