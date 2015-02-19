﻿using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("HTTPОтвет", "HTTPResponse")]
    public class HttpResponseContext : AutoContext<HttpResponseContext>
    {
        private MapImpl _headers = new MapImpl();
        // TODO: Нельзя выделить массив размером больше чем 2GB
        // поэтому функционал сохранения в файл не должен использовать промежуточный буфер _body
        private byte[] _body;
        
        private string _defaultCharset;
        private string _filename;

        public HttpResponseContext(HttpWebResponse response)
        {
            RetrieveResponseData(response);
        }

        public HttpResponseContext(HttpWebResponse response, string dumpToFile)
        {
            throw new NotImplementedException();
            //RetrieveResponseData(response, dumpToFile);
        }

        private void RetrieveResponseData(HttpWebResponse response)
        {
            using(response)
            {
                StatusCode = (int)response.StatusCode;
                _defaultCharset = response.CharacterSet;

                ProcessHeaders(response.Headers);
                ProcessResponseBody(response);
            }
        }

        private void ProcessHeaders(WebHeaderCollection webHeaderCollection)
        {
            foreach (var item in webHeaderCollection.AllKeys)
            {
                _headers.Insert(ValueFactory.Create(item), ValueFactory.Create(webHeaderCollection[item]));
            }
        }

        private void ProcessResponseBody(HttpWebResponse response)
        {
            if (response.ContentLength == 0)
            {
                _body = new byte[0];
                return;
            }

            using (var stream = response.GetResponseStream())
            {
                const int CHUNK_SIZE = 0x10000;
                var mustRead = response.ContentLength;
                if (mustRead < 0) // TODO: Сделать чтение, если Content-Lenght не указан
                    throw new NotImplementedException("Чтение без указания Content-Lenght пока не реализовано");

                _body = new byte[mustRead];
                int offset = 0;
                
                while (mustRead > 0)
                {
                    int portion = Math.Min(CHUNK_SIZE, (int)mustRead);
                    var read = stream.Read(_body, offset, portion);
                    
                    if (read == 0)
                        break;

                    mustRead -= read;
                    offset += read;
                }

            }
        }

        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers
        {
            get
            {
                return _headers;
            }
        }

        [ContextProperty("КодСостояния", "StatusCode", CanWrite = false)]
        public int StatusCode { get; set; }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString(IValue encoding = null)
        {
            if (_body == null)
                return ValueFactory.Create();

            Encoding enc;
            if (encoding == null)
                enc = Encoding.GetEncoding(_defaultCharset);
            else
                enc = TextEncodingEnum.GetEncoding(encoding);

            return ValueFactory.Create(enc.GetString(_body));

        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public IValue GetBodyAsBinaryData()
        {
            if (_body == null)
                return ValueFactory.Create();

            return new BinaryDataContext(_body);
        }

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            if (_filename == null)
                return ValueFactory.Create();

            return ValueFactory.Create(_filename);
        }

        internal void WriteOut(string output)
        {
            _filename = output;
            using(var fs = new FileStream(_filename, FileMode.Create))
            {
                fs.Write(_body, 0, _body.Length);
                _body = null;
            }
        }
    }
}
