﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("Соответствие", "Map")]
    class MapImpl : AutoContext<MapImpl>, ICollectionContext
    {
        private Dictionary<IValue, IValue> _content = new Dictionary<IValue, IValue>(new GenericIValueComparer());

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            IValue result;
            if (!_content.TryGetValue(index, out result))
            {
                result = ValueFactory.Create();
                _content.Add(index, result);
            }

            return result;
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _content[index] = val;
        }

        public override bool IsPropReadable(int propNum)
        {
            return false;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }
        
        #region ICollectionContext Members

        [ContextMethod("Вставить", "Insert")]
        public void Insert(IValue key, IValue val)
        {
            SetIndexedValue(key, val);
        }

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _content.Count;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _content.Clear();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue key)
        {
            _content.Remove(key);
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IValue> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _content)
            {
                yield return new KeyAndValueImpl(item.Key, item.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ScriptConstructor]
        public static MapImpl Constructor()
        {
            return new MapImpl();
        }
    }
}
