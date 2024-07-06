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
#include "stdafx.h"
#include "WordPredictorCom.h"

// CWordPredictorCom

STDMETHODIMP CWordPredictorCom::Create(BSTR basePath)
{
	TRACE(_T("Creating framework...\n"));
	if (0 != _framework.Create(basePath))
	{
		TRACE(_T("Error creating framework\n"));
		return 1;
	}
	TRACE(_T("Created framework\n"));
	
	// Install new packages
	_framework.PACKAGE_INSTALLNEW();

	// DEBUG
	//_framework.PACKAGE_GETAVAILABLE();
	//_framework.PACKAGE_UNINSTALL_ALL();
	//_framework.PACKAGE_INSTALL_NEW();
	//_framework.PACKAGE_GETINSTALLED();
	//_framework.COMPONENT_GETAVAILABLE();
	//_framework.COMPONENT_GETLOADED();
	//_framework.DICTIONARY_SETACTIVELIST(_T("lavlv,engus"));
	//_framework.DICTIONARY_GETLIST();

	return S_OK;
}


STDMETHODIMP CWordPredictorCom::Destroy()
{
	TRACE(_T("Destroying framework...\n"));
	_framework.Destroy();
	TRACE(_T("Destroyed framework.\n"));

	return S_OK;
}

STDMETHODIMP CWordPredictorCom::ProcessRequest(SAFEARRAY *requestMeta, SAFEARRAY *requestData, SAFEARRAY **responseData, int *responseCode)
{
	int result = S_OK;
	TRACE(_T("Processing request...\n"));

	CComSafeArray<byte> inMeta;
	inMeta.Create();
	inMeta.Attach(requestMeta);
	CComSafeArray<BSTR> inData;
	inData.Create();
	if (requestData != NULL)
	{
		inData.Attach(requestData);
	}

	_outData.Create(0L, 0L);

	if (inMeta.GetCount() > 0)
	{
		switch (inMeta[0])
		{
			case REQUEST_RESET_INPUT:
				result = ProcessReset(inMeta); break;
			case REQUEST_INSERT_STRING:
				result = ProcessInsertString(inMeta, inData); break;
			case REQUEST_MOVE_CURSOR:
				result = ProcessMoveCursorRelative(inMeta); break;
			case REQUEST_REMOVE_CHARS:
				result = ProcessRemoveChars(inMeta); break;
			case REQUEST_INSERT_SUGGESTION:
				result = ProcessInsertSuggestion(inMeta); break;
			case REQUEST_CONFIGURE_LEARNING:
				result = ProcessConfigureLearning(inMeta); break;
			case REQUEST_SET_CURSOR:
				result = ProcessSetCursor(inMeta); break;
			case REQUEST_GET_SUGGESTIONS:
				result = CreateSuggestionsResponse(); break;
			case REQUEST_INSTALL_PACKAGES:
				result = ProcessInstallPackages(); break;
			case REQUEST_UNINSTALL_PACKAGES:
				result = ProcessUninstallPackages(); break;
			case REQUEST_SET_ACTIVE_DICTIONARIES:
				result = ProcessSetActiveDictionaries(inMeta, inData); break;
			default:
				result = RESPONSE_ERROR_UNRECOGNISED_MSG_TYPE; break;
		}

	}

	*responseCode = result;
	*responseData = _outData.Detach();	

	inMeta.Detach();
	inData.Detach();

	TRACE(_T("Processed request.\n"));

	return S_OK;
}

// Reset the word prediction buffer
int CWordPredictorCom::ProcessReset(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	if (KPTRESULT_ISSUCCESS(_framework.INPUTMGR_RESET()))
	{		
		if (inMeta.GetCount() > 1)
		{
			if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
			{
				result = CreateSuggestionsResponse();
			}
		}
	}
	else
	{
		result = RESPONSE_ERROR_RESET;
	}

	return result;
}

// Insert a suggestion
int CWordPredictorCom::ProcessInsertString(CComSafeArray<byte> &inMeta, CComSafeArray<BSTR> &inData)
{
	int result = S_OK;

	if (inData.GetCount() > 0 && 
		KPTRESULT_ISSUCCESS(_framework.INPUTMGR_INSERTSTRING(inData[0], inData[0].Length())))
	{
		if (inMeta.GetCount() > 1)
		{
			if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
			{
				result = CreateSuggestionsResponse();
			}
		}
	}
	else
	{
		result = RESPONSE_ERROR_INSERT_STRING;
	}

	return result;
}

// Move the cursor by a relative number of characters
int CWordPredictorCom::ProcessMoveCursorRelative(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	// Index 2 = how many chars to move left. 
	// Index 3 = how many chars to move right. 
	if (inMeta.GetCount() > 3 && 
		KPTRESULT_ISSUCCESS(_framework.INPUTMGR_MOVECURSOR(eKPTSeekRelative, (int)inMeta[3] - (int)inMeta[2])))
	{
		if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
		{
			result = CreateSuggestionsResponse();
		}
	}
	else
	{
		result = RESPONSE_ERROR_MOVE_CURSOR;
	}

	return result;
}

// Remove characters from the prediction buffer
int CWordPredictorCom::ProcessRemoveChars(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	// Index 2 and 3 are how many chars to backspace and delete respectively
	if (inMeta.GetCount() > 3 &&
		KPTRESULT_ISSUCCESS(_framework.INPUTMGR_REMOVE(inMeta[2], inMeta[3])))
	{
		if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
		{
			result = CreateSuggestionsResponse();
		}
	}
	else
	{
		result = RESPONSE_ERROR_REMOVE_CHARS;
	}

	return result;
}

// Insert a suggestion with the specified index
int CWordPredictorCom::ProcessInsertSuggestion(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	// Index 2 is the zero-based suggestion index (not ID)
	if (inMeta.GetCount() > 2 &&
		KPTRESULT_ISSUCCESS(_framework.INPUTMGR_INSERTSUGG(inMeta[2])))
	{
		if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
		{
			result = CreateSuggestionsResponse();
		}
	}
	else
	{
		result = RESPONSE_ERROR_INSERT_SUGGESTION;
	}

	return result;
}

// Enable or disable learning
int CWordPredictorCom::ProcessConfigureLearning(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	uint32_t options = 0;
	char is_on;

	// Index 1 is whether or not to enable learning (0 = off, 1 = on)
	if (inMeta.GetCount() > 1 &&
		KPTRESULT_ISSUCCESS(_framework.LEARN_GETOPTIONS(options)))
	{
		// Toggle learning option if it needs changing
		is_on = (options & eKPTLearnEnabled) != 0 ? 1 : 0;
		if (is_on != inMeta[1] && !KPTRESULT_ISSUCCESS(_framework.LEARN_SETOPTIONS(options ^ eKPTLearnEnabled)))
		{
			result = RESPONSE_ERROR_CONFIGURE_LEARNING;
		}
	}
	else
	{
		result = RESPONSE_ERROR_CONFIGURE_LEARNING;
	}

	return result;
}

// Move the cursor to an absolute location
int CWordPredictorCom::ProcessSetCursor(CComSafeArray<byte> &inMeta)
{
	int result = S_OK;

	// Index 2 is the index to move to
	if (inMeta.GetCount() > 2 &&
		KPTRESULT_ISSUCCESS(_framework.INPUTMGR_MOVECURSOR(eKPTSeekStart, (int)inMeta[2])))
	{
		if (inMeta[1] == REQUEST_GET_SUGGESTIONS)
		{
			result = CreateSuggestionsResponse();
		}
	}
	else
	{
		result = RESPONSE_ERROR_SET_CURSOR;
	}

	return result;
}

// Install any new packages in the packages folder
int CWordPredictorCom::ProcessInstallPackages()
{
	int result = S_OK;

	if (!KPTRESULT_ISSUCCESS(_framework.PACKAGE_INSTALLNEW()))
	{
		result = RESPONSE_ERROR_INSTALL_PACKAGES;
	}

	return result; 
}

// Uninstall all packages
int CWordPredictorCom::ProcessUninstallPackages()
{
	int result = S_OK;

	if (!KPTRESULT_ISSUCCESS(_framework.PACKAGE_UNINSTALLALL()))
	{
		result = RESPONSE_ERROR_UNINSTALL_PACKAGES;
	}

	return result;
}

// Set the list of active dictionaries
int CWordPredictorCom::ProcessSetActiveDictionaries(CComSafeArray<byte> &inMeta, CComSafeArray<BSTR> &inData)
{
	int result = S_OK;

	if (inData.GetCount() > 0 && 
		KPTRESULT_ISSUCCESS(_framework.DICTIONARY_SETACTIVELIST(inData[0])))
	{
		result = S_OK;
	}
	else
	{
		result = RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES;		
	}

	return result;
}

// Create a message containing word suggestions to send to the client
int CWordPredictorCom::CreateSuggestionsResponse()
{
	int result = S_OK;
	size_t sugLoop;
	const KPTUniCharT *pStr;
	KPTInpMgrCurrentWordT currentWord = { 0 };
	const KPTUniCharT *pPrefix = NULL;
	const KPTUniCharT *pSuffix = NULL;
	
	TRACE(_T("Creating suggestions response...\n"));

	// Get the current word details
	if (KPTRESULT_ISSUCCESS(_framework.INPUTMGR_GETCURRWORD(currentWord)))
	{
		pPrefix = currentWord.fixedPrefix;
		pSuffix = currentWord.fixedSuffix;
	}

	// Write the prefix and suffix to the response
	WriteStringIntoResponse(pPrefix);
	WriteStringIntoResponse(pSuffix);	

	if (KPTRESULT_ISSUCCESS(_framework.SUGGS_GETSUGGESTIONS()))
	{
		const KPTSuggWordsReplyT suggReply = _framework.GetCurrentSuggestions();
		for (sugLoop = 0; sugLoop < suggReply.count; sugLoop++)
		{
			pStr = suggReply.suggestions[sugLoop].suggestionString;
			//TRACE(_T("Suggestion: %s\n"), pStr);

			// Write the string into the response
			WriteStringIntoResponse(pStr);
		}
	}
	else
	{
		//TRACE(_T("Suggestions error\n")); 
		result = RESPONSE_ERROR_GET_SUGGESTIONS;
	}

	TRACE(_T("Created suggestions response.\n"));

	return result;
}

// Write a string into the response buffer
void CWordPredictorCom::WriteStringIntoResponse(const wchar_t *pStr)
{
	if (pStr != NULL)
	{
		_outData.Add(::SysAllocString(pStr), 0);
	}
	else
	{
		_outData.Add(::SysAllocString(_T("")), 0);
	}	
}

/*
STDMETHODIMP CWordPredictorCom::TestIn(SAFEARRAY *input, int *result)
{
	if (input != NULL)
	{
		CComSafeArray<BSTR> sa;
		sa.Create();
		sa.Attach(input);
		*result = sa.GetCount();

		for (int i = 0; i < sa.GetCount(); i++)
		{
			TRACE(_T("%s\n"), sa[i]);
		}

		sa.Detach();
	}
	else
	{
		*result = 0;
	}

	return S_OK;
}

STDMETHODIMP CWordPredictorCom::TestOut(SAFEARRAY **output)
{
	CComSafeArray<BSTR> saOutput;
	saOutput.Add(::SysAllocString(_T("Test")), 0);
	saOutput.Add(::SysAllocString(_T("Test 2")), 0);

	*output = saOutput.Detach();

	return S_OK;
}
*/
