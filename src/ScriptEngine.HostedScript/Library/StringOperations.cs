﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Операции с строками")]
    public class StringOperations : GlobalContextBase<StringOperations>
    {
        /// <summary>
        /// Определяет, что строка начинается с указанной подстроки.
        /// </summary>
        /// <param name="inputString">Строка, начало которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое начало строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрНачинаетсяС", "StrStartWith")]
        public bool StrStartWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.StartsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрНачинаетсяС): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Определяет, заканчивается ли строка указанной подстрокой.
        /// </summary>
        /// <param name="inputString">Строка, окончание которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое окончание строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрЗаканчиваетсяНа", "StrEndsWith")]
        public bool StrEndsWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.EndsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрЗаканчиваетсяНа): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Разделяет строку на части по указанным символам-разделителям.
        /// </summary>
        /// <param name="inputString">Разделяемая строка.</param>
        /// <param name="stringDelimiter">Строка символов, каждый из которых является индивидуальным разделителем.</param>
        /// <param name="includeEmpty">Указывает необходимость включать в результат пустые строки, которые могут образоваться в результате разделения исходной строки. Значение по умолчанию: Истина. </param>
        [ContextMethod("СтрРазделить", "StrSplit")]
        public ArrayImpl StrSplit(string inputString, string stringDelimiter, bool includeEmpty = true)
        {
            ArrayImpl arrResult = new ArrayImpl();
            string[] arrParsed;
            if(!string.IsNullOrEmpty(inputString))
            {
                if(!string.IsNullOrEmpty(stringDelimiter))
                {
                    arrParsed = inputString.Split(new string[] { stringDelimiter }, includeEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    arrParsed = new string[] { inputString };
                }
            } else
            {
                arrParsed = new string[] { string.Empty };
            }
            arrResult = new ArrayImpl(arrParsed.Select(x => ValueFactory.Create(x)));
            return arrResult;
        }

        public static IAttachableContext CreateInstance()
        {
            return new StringOperations();
        }

    }
}
