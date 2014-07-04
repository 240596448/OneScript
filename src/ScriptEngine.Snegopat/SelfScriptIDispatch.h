#pragma once

#include "RefCountable.h"
#include "Snegopat_h.h"
#include <vcclr.h>

class SelfScriptIDispatch : 
	public RefCountable,
	public ITypeInfo,
	public IDispatch
{
private:
	gcroot<ScriptEngine::Machine::Contexts::ScriptDrivenObject^> m_scriptDrivenObject;
	gcroot<array<ScriptEngine::Machine::MethodInfo,1>^> m_exportedMeths;

	ScriptEngine::Machine::IValue^ ConvertVariantToScriptArg(VARIANT);
	VARIANT ConvertScriptArgToVariant(ScriptEngine::Machine::IValue^, VARTYPE);
	
public:
	SelfScriptIDispatch(ScriptEngine::Machine::Contexts::ScriptDrivenObject^ self);
	virtual ~SelfScriptIDispatch(void);


	//IUnknown interface -------------------
    virtual HRESULT  __stdcall QueryInterface(
                                REFIID riid, 
                                void **ppObj);
    virtual ULONG   __stdcall AddRef();
    virtual ULONG   __stdcall Release();
    

	//IDispatch interface ---------------
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
        ::DISPPARAMS *pDispParams,
        VARIANT *pVarResult,
        ::EXCEPINFO *pExcepInfo,
        UINT *puArgErr);
	
	// ITypeInfo ---------------------
	virtual HRESULT STDMETHODCALLTYPE GetTypeAttr( 
            ::TYPEATTR **ppTypeAttr);
        
    virtual HRESULT STDMETHODCALLTYPE GetTypeComp( 
        __RPC__deref_out_opt ITypeComp **ppTComp)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetFuncDesc( 
        UINT index,
        ::FUNCDESC **ppFuncDesc);

    virtual HRESULT STDMETHODCALLTYPE GetVarDesc( 
        UINT index,
        ::VARDESC **ppVarDesc)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetNames( 
        MEMBERID memid,
        BSTR *rgBstrNames,
        UINT cMaxNames,
        UINT *pcNames)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetRefTypeOfImplType( 
        UINT index,
        __RPC__out HREFTYPE *pRefType)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetImplTypeFlags( 
        UINT index,
        __RPC__out INT *pImplTypeFlags)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetIDsOfNames( 
        
        __RPC__in_ecount(cNames)  LPOLESTR *rgszNames,
        UINT cNames,
        MEMBERID *pMemId)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE Invoke( 
        PVOID pvInstance,
        MEMBERID memid,
        WORD wFlags,
        ::DISPPARAMS *pDispParams,
        VARIANT *pVarResult,
        ::EXCEPINFO *pExcepInfo,
        UINT *puArgErr)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetDocumentation( 
        ::MEMBERID memid,
        BSTR *pBstrName,
        BSTR *pBstrDocString,
        DWORD *pdwHelpContext,
        BSTR *pBstrHelpFile)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetDllEntry( 
        MEMBERID memid,
        ::INVOKEKIND invKind,
        BSTR *pBstrDllName,
        BSTR *pBstrName,
        WORD *pwOrdinal)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetRefTypeInfo( 
        HREFTYPE hRefType,
        __RPC__deref_out_opt ITypeInfo **ppTInfo)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE AddressOfMember( 
        ::MEMBERID memid,
        ::INVOKEKIND invKind,
        PVOID *ppv)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE CreateInstance( 
        IUnknown *pUnkOuter,
        REFIID riid,
        PVOID *ppvObj)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetMops( 
        MEMBERID memid,
        __RPC__deref_out_opt BSTR *pBstrMops)
    {
		return E_NOTIMPL;
    }
        
    virtual HRESULT STDMETHODCALLTYPE GetContainingTypeLib( 
        ITypeLib **ppTLib,
        UINT *pIndex)
    {
		return E_NOTIMPL;
    }
        
    virtual void STDMETHODCALLTYPE ReleaseTypeAttr( 
        ::TYPEATTR *pTypeAttr)
    {
		delete pTypeAttr;
    }
        
    virtual void STDMETHODCALLTYPE ReleaseFuncDesc( 
        ::FUNCDESC *pFuncDesc)
    {
		delete pFuncDesc;
    }
        
    virtual void STDMETHODCALLTYPE ReleaseVarDesc( 
        ::VARDESC *pVarDesc)
    {
		;
    }


};

