﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("КоллекцияКолонокТаблицыЗначений", "ValueTableColumnCollection")]
    class ValueTableColumnCollection : DynamicPropertiesAccessor, ICollectionContext
    {
        private List<ValueTableColumn> _columns = new List<ValueTableColumn>();
        private int _internal_counter = 0; // Нарастающий счётчик определителей колонок

        public ValueTableColumnCollection()
        {
        }

        [ContextMethod("Добавить", "Add")]
        public ValueTableColumn Add(string Name, IValue Type = null, string Title = null)
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            var Width = 0; // затычка

            ValueTableColumn column = new ValueTableColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Add(column);

            return column;
        }

        [ContextMethod("Вставить", "Insert")]
        public ValueTableColumn Insert(int index, string Name, IValue Type = null)
            // TODO: добавить Title и Width после того, как количество обрабатываемых параметров будет увеличено хотя бы до 5
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            var Title = Name; // TODO: Затычка
            var Width = 0; // TODO: Затычка

            ValueTableColumn column = new ValueTableColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Insert(index, column);

            return column;
        }

        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueTableColumn column)
        {
            return _columns.IndexOf(column);
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _columns.Count;
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(string Name)
        {
            var column = FindColumnByName(Name);
            if (column == null)
                return ValueFactory.Create();
            return column;
        }

        public ValueTableColumn FindColumnByName(string Name)
        {
            var Comparer = StringComparer.OrdinalIgnoreCase;
            return _columns.Find(column => Comparer.Equals(Name, column.Name));
        }
        public ValueTableColumn FindColumnById(int id)
        {
            return _columns.Find(column => column.ID == id);
        }

        public ValueTableColumn FindColumnByIndex(int index)
        {
            return _columns[index];
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _columns)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public override int FindProperty(string name)
        {
            ValueTableColumn C = FindColumnByName(name);
            if (C == null)
                throw RuntimeException.PropNotFoundException(name);
            return C.ID;
        }

        public override IValue GetPropValue(int propNum)
        {
            return FindColumnById(propNum);
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public ValueTableColumn GetColumnByIIndex(IValue index)
        {
            if (index.DataType == DataType.String)
            {
                ValueTableColumn C = FindColumnByName(index.AsString());
                if (C == null)
                    throw RuntimeException.PropNotFoundException(index.AsString());
                return C;
            }

            if (index.DataType == DataType.Number)
            {
                int i_index = Decimal.ToInt32(index.AsNumber());
                if (i_index < 0 || i_index >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                ValueTableColumn C = FindColumnByIndex(i_index);
                return C;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetColumnByIIndex(index);
        }

        private static ContextMethodsMapper<ValueTableColumnCollection> _methods = new ContextMethodsMapper<ValueTableColumnCollection>();

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                retValue = binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }
    }
}
