﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class PropertyNameIndexAccessor : ContextIValueImpl
    {
        public PropertyNameIndexAccessor()
        {

        }

        public PropertyNameIndexAccessor(TypeDescriptor type):base(type)
        {
        }

        public override bool IsIndexed
        {
            get { return true; }
        }

        public override bool IsPropReadable(int propNum)
        {
            return false;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.DataType != DataType.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = FindProperty(index.AsString());
            if (IsPropReadable(n))
                return GetPropValue(n);
            else
                throw RuntimeException.PropIsNotReadableException(index.AsString());
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.DataType != DataType.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = FindProperty(index.AsString());
            if (IsPropWritable(n))
                SetPropValue(n, val);
            else
                throw RuntimeException.PropIsNotWritableException(index.AsString());
        }
    }
}
