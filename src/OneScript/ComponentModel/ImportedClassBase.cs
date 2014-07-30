﻿using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    public abstract class ImportedClassBase : IValue
    {
        DataType _type;

        internal void SetDataType(DataType type)
        {
            SetDataTypeInternal(type);
        }

        protected DataType GetDataTypeInternal()
        {
            return _type;
        }

        protected void SetDataTypeInternal(DataType type)
        {
            System.Diagnostics.Debug.Assert(_type == null);
            _type = type;
        }

        public override string ToString()
        {
            return _type != null ? _type.ToString() : base.ToString();
        }

        public virtual DataType Type
        {
            get { return _type; }
        }

        public virtual double AsNumber()
        {
            throw TypeConversionException.ConvertToNumberException();
        }

        public virtual string AsString()
        {
            return ToString();
        }

        public virtual DateTime AsDate()
        {
            throw TypeConversionException.ConvertToDateException();
        }

        public virtual bool AsBoolean()
        {
            throw TypeConversionException.ConvertToBooleanException();
        }

        public virtual IRuntimeContextInstance AsObject()
        {
            throw TypeConversionException.ConvertToObjectException();
        }

        public virtual bool Equals(IValue other)
        {
            if (other == null)
                return false;

            return this.Equals((object)other);
        }

        public int CompareTo(IValue other)
        {
            if (other == null)
            {
                return _type == null ? 0 : 1;
            }

            if(_type != null)
            {
                if(_type == other.Type)
                {
                    return CompareSameType(other);
                }
                else
                {
                    return _type.CompareTo(other.Type);
                }
            }
            else
            {
                return -1;
            }
        }

        protected virtual int CompareSameType(IValue other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }
    }
}
