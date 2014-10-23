﻿using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    class CLREnumValueWrapper<T> : EnumerationValue
    {
        T _realValue;

        public CLREnumValueWrapper(EnumerationContext owner, T realValue):base(owner)
        {

        }

        public T UnderlyingObject
        {
            get
            {
                return _realValue;
            }
        }

        public override bool Equals(Machine.IValue other)
        {
            var otherWrapper = other as CLREnumValueWrapper<T>;
            if (otherWrapper == null)
                return false;

            return _realValue.Equals(other);
        }
    }
}
