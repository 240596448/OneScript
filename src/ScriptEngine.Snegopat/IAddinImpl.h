#pragma once

#include <comdef.h>
#include "Snegopat_h.h"
#include "RefCountable.h"
#include <vcclr.h>

class IAddinImpl :
	public RefCountable,
	public IAddinMacroses,
	public IDispatch
{
private:
	gcroot<ScriptEngine::Machine::Contexts::UserScriptContextInstance^> m_innerObject;
	BSTR m_uniqueName;
	BSTR m_displayName;
	BSTR m_fullPath;

public:
	
	IAddinImpl(ScriptEngine::Machine::Contexts::UserScriptContextInstance^ innerObject);

	void SetNames(BSTR uniqueName, BSTR displayName, BSTR fullPath)
	{
		m_uniqueName = uniqueName;
		m_displayName = displayName;
		m_fullPath = fullPath;
	}

	ScriptEngine::Machine::Contexts::UserScriptContextInstance^
		GetManagedInstance()
	{
		return m_innerObject;
	}

	//IUnknown interface 
    virtual HRESULT  __stdcall QueryInterface(
                                REFIID riid, 
                                void **ppObj);
    virtual ULONG   __stdcall AddRef();
    virtual ULONG   __stdcall Release();
    

	//IDispatch interface
	virtual HRESULT STDMETHODCALLTYPE GetTypeInfoCount( 
             UINT *pctinfo);
        
    virtual HRESULT STDMETHODCALLTYPE GetTypeInfo( 
         UINT iTInfo,
         LCID lcid,
         ITypeInfo **ppTInfo);
        
    virtual HRESULT STDMETHODCALLTYPE GetIDsOfNames( 
        REFIID riid,
        LPOLESTR *rgszNames,
        UINT cNames,
        LCID lcid,
        DISPID *rgDispId);
        
    virtual HRESULT STDMETHODCALLTYPE Invoke( 
        DISPID dispIdMember,
        REFIID riid,
        LCID lcid,
        WORD wFlags,
        DISPPARAMS *pDispParams,
        VARIANT *pVarResult,
        EXCEPINFO *pExcepInfo,
        UINT *puArgErr);

    virtual HRESULT STDMETHODCALLTYPE macroses(SAFEARRAY **result);
        
    virtual HRESULT STDMETHODCALLTYPE invokeMacros(BSTR MacrosName, VARIANT *result);
        
	virtual void OnZeroCount();

	~IAddinImpl(void);

};

