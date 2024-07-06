#pragma once
/******************************************************************************
*
* Copyright 2019 Tim Brogden
* All rights reserved. The Keysticks software and accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* KeyPoint Technologies (UK) Ltd - OpenAdaptxt API and implementation
* Tim Brogden - WordPredictor COM component with demo application
*
*****************************************************************************/
#include "resource.h"       // main symbols

#include "WordPredictor_i.h"

// WordPredictor includes
#include "Constants.h"
#include "FrameworkWrapper.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CWordPredictorCom

class ATL_NO_VTABLE CWordPredictorCom :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CWordPredictorCom, &CLSID_WordPredictorCom>,
	public IDispatchImpl<IWordPredictorCom, &IID_IWordPredictorCom, &LIBID_WordPredictorLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CWordPredictorCom()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_WORDPREDICTORCOM)


BEGIN_COM_MAP(CWordPredictorCom)
	COM_INTERFACE_ENTRY(IWordPredictorCom)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

	STDMETHOD(Create)(BSTR bstrBasePath);
	STDMETHOD(Destroy)();
	STDMETHOD(ProcessRequest)(SAFEARRAY *requestMeta, SAFEARRAY *requestData, SAFEARRAY **responseData, int *responseCode);
	//STDMETHOD(TestIn)(SAFEARRAY *input, int *result);
	//STDMETHOD(TestOut)(SAFEARRAY **output);

private:

	FrameworkWrapper _framework;
	CComSafeArray<BSTR> _outData;

	int ProcessReset(CComSafeArray<byte> &inMeta);
	int ProcessInsertString(CComSafeArray<byte> &inMeta, CComSafeArray<BSTR> &inData);
	int ProcessMoveCursorRelative(CComSafeArray<byte> &inMeta);
	int ProcessRemoveChars(CComSafeArray<byte> &inMeta);
	int ProcessInsertSuggestion(CComSafeArray<byte> &inMeta);
	int ProcessConfigureLearning(CComSafeArray<byte> &inMeta);
	int ProcessSetCursor(CComSafeArray<byte> &inMeta);
	int ProcessInstallPackages();
	int ProcessUninstallPackages();
	int ProcessSetActiveDictionaries(CComSafeArray<byte> &inMeta, CComSafeArray<BSTR> &inData);

	int CreateSuggestionsResponse();
	void WriteStringIntoResponse(const wchar_t *pStr);

};

OBJECT_ENTRY_AUTO(__uuidof(WordPredictorCom), CWordPredictorCom)
