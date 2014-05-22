﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("Массив")]
    class ArrayImpl : ContextBase<ArrayImpl>, ICollectionContext
    {
        private List<IValue> _values;

        public ArrayImpl()
        {
            _values = new List<IValue>();
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return _values[(int)index.AsNumber()];
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _values[(int)index.AsNumber()] = val;
        }

        public override bool IsPropReadable(int propNum)
        {
            throw new NotImplementedException();
        }

        public override bool IsPropWritable(int propNum)
        {
            throw new NotImplementedException();
        }

        #region ICollectionContext Members
        
        [ContextMethod("Количество")]
        public int Count()
        {
            return _values.Count;
        }

        [ContextMethod("Очистить")]
        public void Clear()
        {
            _values.Clear();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return item;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ContextMethod("Добавить")]
        public void Add(IValue value)
        {
            _values.Add(value);
        }

        [ContextMethod("Вставить")]
        public void Insert(int index, IValue value)
        {
            _values.Insert(index, value);
        }

        [ContextMethod("Найти")]
        public IValue Find(IValue what)
        {
            var idx = _values.FindIndex(x => x.Equals(what));
            if(idx < 0)
            {
                return ValueFactory.Create();
            }
            else
            {
                return ValueFactory.Create(idx);
            }
        }

        [ContextMethod("Удалить")]
        public void Remove(int index)
        {
            _values.RemoveAt(index);
        }

        [ContextMethod("ВГраница")]
        public int UpperBound()
        {
            return _values.Count - 1;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ArrayImpl();
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue[] dimensions)
        {
            ArrayImpl cloneable = null;
            for (int dim = dimensions.Length - 1; dim >= 0; dim--)
            {
                int bound = (int)dimensions[dim].AsNumber();
                var newInst = new ArrayImpl();
                FillArray(newInst, bound);
                if(cloneable != null)
                {
                    for (int i = 0; i < bound; i++)
                    {
                        newInst._values[i] = CloneArray(cloneable);
                    }
                }
                cloneable = newInst;
                
            }

            return cloneable;

        }

        private static void FillArray(ArrayImpl currentArray, int bound)
        {
            for (int i = 0; i < bound; i++)
            {
                currentArray._values.Add(ValueFactory.Create());
            }
        }
        
        private static IValue CloneArray(ArrayImpl cloneable)
        {
            ArrayImpl clone = new ArrayImpl();
            foreach (var item in cloneable._values)
            {
                if (item.DataType == DataType.Undefined)
                    clone._values.Add(ValueFactory.Create());
                else
                    clone._values.Add(item);
            }
            return clone;
        }

    }
}
