#pragma once
/******************************************************************************
*
* Copyright 2019 Tim Brogden
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* KeyPoint Technologies (UK) Ltd - OpenAdaptxt API and implementation
* Tim Brogden - WordPredictor COM component with demo application
*
*****************************************************************************/

#include <Windows.h>
#include "kptapi_framework.h"
#include "kptapi_error.h"
#include "kptapi_components.h"
#include "kptapi_componenttypes.h"
#include "kptapi_packages.h"
#include "kptapi_dict.h"
#include "kptapi_suggs.h"
#include "kptapi_inputmgr.h"
#include "kptapi_learn.h"


	#define OpenAdaptxtDLLName "kptframeworkv2DMD.dll"
	#define KPTFwkCreateName "KPTFwkCreate"
	#define KPTFwkDestroyName "KPTFwkDestroy"
	#define KPTFwkRunCmdName "KPTFwkRunCmd"
	#define KPTFwkReleaseAllocName "KPTFwkReleaseAlloc"

	typedef KPTResultT (KPT_CALL *KPTFwkCreateFunction)(const KPTCreateParamsT* aCreate);
	typedef KPTResultT (KPT_CALL *KPTFwkParameterlessFunction)(void);
	typedef KPTResultT (KPT_CALL *KPTFwkRunCmdFunction)(uint32_t aCommand, intptr_t aFirst, intptr_t aSecond);
	typedef KPTResultT (KPT_CALL *KPTFwkReleaseAllocFunction)(void *aAllocT);

	// Wrapper to manage calls to the OpenAdaptxt word prediction DLL
	// The code in this class is closely based upon the examples in the OpenAdaptxt help file
	class FrameworkWrapper
	{
	private:
		bool _isCreated;
		bool _isSuggestionsAllocated;
		KPTSuggWordsReplyT _suggestions;
		HINSTANCE _dllHandle;
		KPTFwkCreateFunction _callKPTFwkCreate;
		KPTFwkParameterlessFunction _callKPTFwkDestroy;
		KPTFwkRunCmdFunction _callKPTFwkRunCmd;
		KPTFwkReleaseAllocFunction _callKPTFwkReleaseAlloc;

	public:
		FrameworkWrapper(void);
		~FrameworkWrapper(void);

		int Create(const KPTSysCharT *pBasePath);
		void Destroy(void);
		const KPTSuggWordsReplyT &GetCurrentSuggestions();

		KPTResultT PACKAGE_GETAVAILABLE(void);
		KPTResultT PACKAGE_GETINSTALLED(void);
		KPTResultT PACKAGE_INSTALLNEW(void);
		KPTResultT PACKAGE_UNINSTALLALL(void);
		KPTResultT COMPONENT_GETAVAILABLE(void);
		KPTResultT COMPONENT_GETLOADED(void);
		KPTResultT DICTIONARY_GETLIST(void);	
		KPTResultT DICTIONARY_SETACTIVELIST(const KPTUniCharT *dictList);
		KPTResultT SUGGS_GETCONFIG(void);
		KPTResultT INPUTMGR_RESET(void);
		KPTResultT INPUTMGR_INSERTCHAR(KPTUniCharT ch);
		KPTResultT INPUTMGR_INSERTSTRING(const KPTUniCharT *str, size_t numChars);
		KPTResultT INPUTMGR_MOVECURSOR(KPTInpMgrCursorMoveT moveType, int moveAmount);
		KPTResultT INPUTMGR_REMOVE(size_t numBefore, size_t numAfter);
		KPTResultT INPUTMGR_INSERTSUGG(size_t suggestionIndex);
		KPTResultT INPUTMGR_GETCURRWORD(KPTInpMgrCurrentWordT &currentWord);
		KPTResultT SUGGS_GETSUGGESTIONS();
		KPTResultT LEARN_GETOPTIONS(uint32_t &options);
		KPTResultT LEARN_SETOPTIONS(uint32_t options);

	private:
		void ShowList(KPTDictListAllocT* aList);
	};

