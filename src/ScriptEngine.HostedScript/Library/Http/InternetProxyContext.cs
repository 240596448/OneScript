﻿using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    [ContextClass("ИнтернетПрокси", "InternetProxy")]
    public class InternetProxyContext : AutoContext<InternetProxyContext>
    {
        IWebProxy _proxy;
        NetworkCredential _creds;
        bool _isDefault;
        ArrayImpl _bypassProxyOnAddresses;
        bool _bypassLocal;

        public InternetProxyContext(bool useDefault)
        {
            _isDefault = useDefault;
            if (useDefault)
            {
                _proxy = WebRequest.GetSystemWebProxy();
                _creds = (NetworkCredential)_proxy.Credentials;
                if (_creds == null)
                    _creds = new NetworkCredential();
            }
            else
            {
                _proxy = new WebProxy();
                _bypassLocal = ((WebProxy)_proxy).BypassProxyOnLocal;
                _creds = new NetworkCredential();
            }

            _bypassProxyOnAddresses = new ArrayImpl();
        }

        public IWebProxy GetProxy()
        {
            if (!_isDefault)
            {
                var wp = (WebProxy)_proxy;
                wp.Credentials = _creds;
                wp.BypassList = _bypassProxyOnAddresses.Select(x => x.AsString()).ToArray();
                wp.BypassProxyOnLocal = _bypassLocal;
            }
            
            return _proxy;
        }

        [ContextProperty("Пользователь","User")]
        public string User 
        {
            get
            {
                return _creds.UserName;
            }
            set
            {
                _creds.UserName = value;
            }
        }

        [ContextProperty("Пароль", "Password")]
        public string Password 
        {
            get
            {
                return _creds.Password;
            }
            set
            {
               _creds.Password = value;
            }
        }

        [ContextProperty("НеИспользоватьПроксиДляАдресов","BypassProxyOnAddresses")]
        public ArrayImpl BypassProxyList
        {
            get
            {
                return _bypassProxyOnAddresses;
            }
            set
            {
                _bypassProxyOnAddresses = value;
            }
        }

        [ContextProperty("НеИспользоватьПроксиДляЛокальныхАдресов", "BypassProxyOnLocal")]
        public bool BypassProxyOnLocal
        {
            get
            {
                return _bypassLocal;
            }
            set
            {
                _bypassLocal = value;
            }
        }

        [ScriptConstructor]
        public static InternetProxyContext Constructor()
        {
            return Constructor(ValueFactory.Create(false));
        }

        [ScriptConstructor]
        public static InternetProxyContext Constructor(IValue useDefault)
        {
            return new InternetProxyContext(useDefault.AsBoolean());
        }
    }
}
