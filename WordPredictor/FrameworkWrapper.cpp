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
#include "StdAfx.h"
#include "FrameworkWrapper.h"

	// Constructor
	FrameworkWrapper::FrameworkWrapper(void)
	{
		_isCreated = false;		
		memset(&_suggestions, 0, sizeof(KPTSuggWordsReplyT));
	}

	// Destructor
	FrameworkWrapper::~FrameworkWrapper(void)
	{
	}

	// Load the framework
	int FrameworkWrapper::Create(const KPTSysCharT *pBasePath)
	{
		KPTResultT result;

		// Get a handle to the DLL module
		_dllHandle = LoadLibrary(TEXT(OpenAdaptxtDLLName)); 
 		if (_dllHandle == NULL)		
		{ 
			return 1;
		}

		// If the handle is valid, try to get the function addresses 
		_callKPTFwkCreate = (KPTFwkCreateFunction)GetProcAddress(_dllHandle, KPTFwkCreateName); 
		_callKPTFwkDestroy = (KPTFwkParameterlessFunction)GetProcAddress(_dllHandle, KPTFwkDestroyName); 
		_callKPTFwkRunCmd = (KPTFwkRunCmdFunction)GetProcAddress(_dllHandle, KPTFwkRunCmdName); 
		_callKPTFwkReleaseAlloc = (KPTFwkReleaseAllocFunction)GetProcAddress(_dllHandle, KPTFwkReleaseAllocName); 
		if (_callKPTFwkCreate == NULL ||
			_callKPTFwkDestroy == NULL ||
			_callKPTFwkRunCmd == NULL ||
			_callKPTFwkReleaseAlloc == NULL)
		{
			FreeLibrary(_dllHandle);
			return 1;
		}
 
		KPTInitT initItems[] =
		{
			{ KPT_CC_FRAMEWORK, 0, KPT_INIT_FRAMEWORK_LOCKINGENABLED, (intptr_t)eKPTTrue},
			{ KPT_CC_FRAMEWORK, 0, KPT_INIT_FRAMEWORK_BASEPATH,       (intptr_t)pBasePath},
		};

		KPTCreateParamsT createParams = { 0 };
		createParams.initItems = initItems;
		createParams.initItemCount = sizeof(initItems) / sizeof(*initItems);

		// Create framework
		result = (_callKPTFwkCreate)(&createParams);
		if (KPTRESULT_FAILED(result))
		{
			FreeLibrary(_dllHandle);
			return 1;
		}

		_isCreated = true;

		return 0;
	}

	// Unload the framework
	void FrameworkWrapper::Destroy()
	{
		if (_isCreated)
		{
			_isCreated = false;

			// Destroy the framework
			(_callKPTFwkDestroy)();

			// Release DLL handle
			FreeLibrary(_dllHandle);

			//TRACE(_T("Destroyed framework\n")); 
		}
	}

	// List the available packages
	KPTResultT FrameworkWrapper::PACKAGE_GETAVAILABLE(void)
	{
		KPTResultT result;
		KPTPackageAvailableListAllocT availablePackages = {0};  // Must initialise all AllocT structures. 

		// Get information on packages found in directory  
		result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_GETAVAILABLE, (intptr_t)&availablePackages, 0);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		// Print out package information 
		if (0 == availablePackages.count)
		{
			TRACE(KPT_TS("No packages found!\n"));
		}
		else
		{
			size_t pkgIndex;
			size_t compIndex;
			for (pkgIndex = 0; pkgIndex < availablePackages.count; pkgIndex++)
			{
				TRACE(KPT_TS("Package Name: %s\n"),
					availablePackages.packages[pkgIndex].packageName);
				TRACE(KPT_TS("\tComponent Count: %u\n"),
					availablePackages.packages[pkgIndex].componentCount);
				TRACE(KPT_TS("\tComponent: ID\tType\tVersion\n"));
				for (compIndex = 0; compIndex < availablePackages.packages[pkgIndex].componentCount; compIndex++)
				{
					TRACE(KPT_TS("\t%u\t%u\t%u\n"),
						availablePackages.packages[pkgIndex].components[compIndex].id,
						availablePackages.packages[pkgIndex].components[compIndex].type,
						availablePackages.packages[pkgIndex].components[compIndex].version);
				}   
			}   
		}
		result = (_callKPTFwkReleaseAlloc)(&availablePackages);

		return result;
	}

	// List the installed packages
	KPTResultT FrameworkWrapper::PACKAGE_GETINSTALLED(void)
	{
		KPTResultT result;
		KPTPackageInstalledListAllocT installedPackages = {0};  // Must initialise all AllocT structures. 

		// Get installed package information 
		result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_GETINSTALLED, (intptr_t)&installedPackages, 0);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		// Print out package information 
		if (0 == installedPackages.count)
		{
			TRACE(KPT_TS("No packages installed!\n"));
		}
		else
		{
			size_t pkgIndex;
			size_t compIndex;
			for (pkgIndex = 0; pkgIndex < installedPackages.count; pkgIndex++)
			{
				TRACE(KPT_TS("Package Name: %s\n"),
					installedPackages.packages[pkgIndex].packageName);
				TRACE(KPT_TS("Package ID: %u\n"),
					installedPackages.packages[pkgIndex].packageId);
				TRACE(KPT_TS("Component Count:%u\n"),
					installedPackages.packages[pkgIndex].componentCount);
				TRACE(KPT_TS("\tComponent: ID\tType\tVersion\n"));
				for (compIndex = 0; compIndex < installedPackages.packages[pkgIndex].componentCount; compIndex++)
				{
					TRACE(KPT_TS("\t%u\t%u\t%u\n"),
						installedPackages.packages[pkgIndex].components[compIndex].id,
						installedPackages.packages[pkgIndex].components[compIndex].type,
						installedPackages.packages[pkgIndex].components[compIndex].version);
				}   
			}   
		}
		result = (_callKPTFwkReleaseAlloc)(&installedPackages);

		return result;
	}

	// Install any new packages in the packages folder
	KPTResultT FrameworkWrapper::PACKAGE_INSTALLNEW(void)
	{
		KPTResultT result = KPTRESULT_SUCCESS;
		size_t pkgIndex;
		size_t instIndex;
		BOOL isInstalled;
		//BOOL isAvailable;
		const KPTSysCharT *packageName;
		KPTPackageInstalledIdT id;
		KPTPackageAvailableListAllocT availablePackages = {0};  // Must initialise all AllocT structures. 
		KPTPackageInstalledListAllocT installedPackages = {0};  // Must initialise all AllocT structures. 
		
		// Find available packages
		// Note: Ignore GETAVAILABLE error - probably means there aren't any packages.
		if (KPTRESULT_ISSUCCESS((_callKPTFwkRunCmd)(KPTCMD_PACKAGE_GETAVAILABLE, (intptr_t)&availablePackages, 0)))
		{
			// Get installed packages
			result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_GETINSTALLED, (intptr_t)&installedPackages, 0);
			if (KPTRESULT_FAILED(result))
			{
				return result;
			}

			// Uninstall packages that are no longer in packages folder
			// Remark: Tried this as a means of telling the server to uninstall packages just by deleting the package files
			// then calling this method, but it doesn't work: uninstalling packages seems to require package files to be present
			/*
			for (instIndex = 0; instIndex < installedPackages.count; instIndex++)
			{
				packageName = installedPackages.packages[instIndex].packageName;
			
				// Check whether available
				isAvailable = false;
				for (pkgIndex = 0; pkgIndex < availablePackages.count; pkgIndex++)
				{
					if (0 == wcsncmp(availablePackages.packages[pkgIndex].packageName, packageName, MAX_PATH))
					{
						isAvailable = true;
						break;
					}
				}

				if (!isAvailable)
				{
					TRACE(KPT_TS("Uninstalling Package: %s\n"), packageName);
					result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_UNINSTALL, (intptr_t)packageName, (intptr_t)&id);
					if (KPTRESULT_FAILED(result))
					{
						TRACE(KPT_TS("Failed to uninstall Package: %s\n"), packageName);
						return result;
					}
					TRACE(KPT_TS("Uninstalled Package: %s\n"), packageName);
				}
			}
			*/
			
			// Install new packages
			for (pkgIndex = 0; pkgIndex < availablePackages.count; pkgIndex++)
			{
				packageName = availablePackages.packages[pkgIndex].packageName;

				// Check whether installed 
				isInstalled = false;
				for (instIndex = 0; instIndex < installedPackages.count; instIndex++)
				{
					if (0 == wcsncmp(packageName, installedPackages.packages[instIndex].packageName, MAX_PATH))
					{
						isInstalled = true;
						break;
					}
				}

				if (!isInstalled)
				{
					TRACE(KPT_TS("Installing Package: %s\n"), packageName);
					result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_INSTALL, (intptr_t)packageName, (intptr_t)&id);
					if (KPTRESULT_FAILED(result))
					{
						TRACE(KPT_TS("Failed to Install Package: %s\n"), packageName);
						return result;
					}
					TRACE(KPT_TS("Installed Package: %s\n"), packageName);
				}
			}

			(_callKPTFwkReleaseAlloc)(&installedPackages);
		}

		(_callKPTFwkReleaseAlloc)(&availablePackages);

		return result;
	}

	// Uninstall all packages
	KPTResultT FrameworkWrapper::PACKAGE_UNINSTALLALL(void)
	{
		KPTResultT result;
		KPTPackageInstalledListAllocT installedPackages = {0};  // Must initialise all AllocT structures. 

		// Get installed package information 
		result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_GETINSTALLED, (intptr_t)&installedPackages, 0);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		if (0 != installedPackages.count)
		{
			size_t pkgIndex;
			for (pkgIndex = 0; pkgIndex < installedPackages.count; pkgIndex++)
			{
				TRACE(KPT_TS("Un-installing Package: %s\n"),
					installedPackages.packages[pkgIndex].packageName);
				result = (_callKPTFwkRunCmd)(KPTCMD_PACKAGE_UNINSTALL, (intptr_t)installedPackages.packages[pkgIndex].packageId, 0);
				if (KPTRESULT_FAILED(result))
				{
					return result;
				}
				TRACE(KPT_TS("Un-installed Package: %s\n"),
						installedPackages.packages[pkgIndex].packageName);
			}        
		}
		result = (_callKPTFwkReleaseAlloc)(&installedPackages);

		return result;
	}

	// List the installed components
	KPTResultT FrameworkWrapper::COMPONENT_GETAVAILABLE(void)
	{
		KPTResultT result;
		size_t index;
		KPTComponentListAllocT available = {0};  // Must initialise all AllocT structures. 
		const KPTComponentExtraDictionaryT* details = NULL;

		// Get a list of the installed components 
		result = (_callKPTFwkRunCmd)(KPTCMD_COMPONENT_GETAVAILABLE, (intptr_t)&available, 0);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		// Access details for component 
		for (index = 0; index<available.count; ++index)
		{
			// Output the details using a system printf function.
			TRACE(KPT_TS("component %d = "), (int)index);
			switch (available.components[index].componentType)
			{
			case KPT_COMPONENTTYPE_ADAPTXTENGINE:
				TRACE(KPT_TS("engine"));
				break;

			case KPT_COMPONENTTYPE_DICTIONARY:
				details = (const KPTComponentExtraDictionaryT*)available.components[index].extraDetails;
				if (details)
				{
					TRACE(KPT_TS("dictionary %s"), details->dictInfo.dictFileName);
				}
				else
				{
					TRACE(KPT_TS("dictionary with no details"));
				}
				break;

			default:
				TRACE(KPT_TS("other"));
				break;
			}

			if (available.components[index].isLoaded)
			{
				TRACE(KPT_TS(" Loaded\n"));
			}
			else
			{
				TRACE(KPT_TS(" Not Loaded\n"));
			}
		}

		result = (_callKPTFwkReleaseAlloc)(&available);
		return result;
	}

	// List the components that are loaded
	KPTResultT FrameworkWrapper::COMPONENT_GETLOADED(void)
	{
		KPTResultT result;
		size_t index;   
		KPTComponentListAllocT components = {0};  // Must initialise all AllocT structures. 
		const KPTComponentExtraDictionaryT* details = NULL; 

		// Get a list of the installed components 
		result = (_callKPTFwkRunCmd)(KPTCMD_COMPONENT_GETLOADED, (intptr_t)&components, 0);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}
    
		// Access details for component 
		for (index = 0; index<components.count; ++index)
		{
			TRACE(KPT_TS("component %d = "), (int)index);
			switch (components.components[index].componentType)
			{
			case KPT_COMPONENTTYPE_ADAPTXTENGINE:
				TRACE(KPT_TS("engine"));
				break;

			case KPT_COMPONENTTYPE_DICTIONARY:
				details = (const KPTComponentExtraDictionaryT*)components.components[index].extraDetails;
				if (details)
				{
					TRACE(KPT_TS("dictionary %s"), details->dictInfo.dictFileName);
				}
				else
				{
					TRACE(KPT_TS("dictionary with no details"));
				}
				break;

			default:
				TRACE(KPT_TS("other"));
				break;
			}

			if (components.components[index].isLoaded)
			{
				TRACE(KPT_TS(" Loaded\n"));
			}
			else
			{
				TRACE(KPT_TS(" Not Loaded\n"));
			}
		}
    
		result = (_callKPTFwkReleaseAlloc)(&components);
		return result;
	}

	// List the dictionaries
	KPTResultT FrameworkWrapper::DICTIONARY_GETLIST(void)
	{
		KPTResultT result;
		KPTDictListAllocT dictionaryList = {0};  // Must initialise all AllocT structures. 
		KPTLanguageMatchingT langFilter = {eKPTLangFiltering, {"en-*-x-dict", NULL}};
		KPTLanguageMatchingT langLookup = {eKPTLangLookup, {"en-*-x-dict", NULL}};

		// Get the list with no matching 
		TRACE(KPT_TS("KPTCMD_DICTIONARY_GETLIST: No matching\n"));
		result = (_callKPTFwkRunCmd)(KPTCMD_DICTIONARY_GETLIST, (intptr_t)&dictionaryList, (intptr_t)NULL);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}
		ShowList(&dictionaryList);

		// Try again using language filtering 
		TRACE(KPT_TS("KPTCMD_DICTIONARY_GETLIST: eKPTLangFiltering\n"));
		result = (_callKPTFwkRunCmd)(KPTCMD_DICTIONARY_GETLIST, (intptr_t)&dictionaryList, (intptr_t)&langFilter);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}
		ShowList(&dictionaryList);

		// Try again using language lookup 
		TRACE(KPT_TS("KPTCMD_DICTIONARY_GETLIST: eKPTLangLookup\n"));
		result = (_callKPTFwkRunCmd)(KPTCMD_DICTIONARY_GETLIST, (intptr_t)&dictionaryList, (intptr_t)&langLookup);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}
		ShowList(&dictionaryList);

		// Only need to release the list once 
		result = (_callKPTFwkReleaseAlloc)(&dictionaryList);
		return result;
	}

	// Set the active dictionaries in the given priority order
	// from a comma-delimited list of dictionary names e.g. enggb,frefr,lavlv
	KPTResultT FrameworkWrapper::DICTIONARY_SETACTIVELIST(const KPTUniCharT *dictList)
	{
		KPTResultT result;
		KPTDictListAllocT dictionaryList = {0};  // Must initialise all AllocT structures. 

		// Get the list of loaded dictionaries
		result = (_callKPTFwkRunCmd)(KPTCMD_DICTIONARY_GETLIST, (intptr_t)&dictionaryList, (intptr_t)NULL);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		// Copy token list string into modifiable string
		KPTUniCharT *nextToken;
		KPTUniCharT dictListCopy[MAX_STR_LEN];
		wcsncpy_s(dictListCopy, dictList, MAX_STR_LEN);

		// Extract dictionary names into an array,
		// ignoring ones that aren't loaded
		BOOL matched;
		size_t dictIndex;
		size_t dictCount = 0;
		KPTUniCharT *dictName;
		KPTUniCharT *token = wcstok_s(dictListCopy, _T(","), &nextToken);
		KPTUniCharT *requiredDictionaries[MAX_DICTIONARIES];
		while (token != NULL && dictCount < MAX_DICTIONARIES) 
		{
			// Check that the dictionary is loaded
			matched = false;
			for (dictIndex = 0; dictIndex < dictionaryList.count; dictIndex++)
			{
				// See if the dictionary is required
				dictName = dictionaryList.dictInfo[dictIndex].dictFileName;
				if (0 == wcsncmp(dictName, token, MAX_STR_LEN))
				{
					matched = true;
					break;
				}
			}

			// Add to required dictionaries list if matched
			if (matched)
			{
				requiredDictionaries[dictCount++] = token;
			}

			token = wcstok_s(NULL, _T(","), &nextToken);
		}

		// Decide whether each dictionary is active and set its priority
		size_t priority;
		for (dictIndex = 0; dictIndex < dictionaryList.count; dictIndex++)
		{
			// See if the dictionary is required
			dictName = dictionaryList.dictInfo[dictIndex].dictFileName;
			for (priority = 0; priority < dictCount; priority++)
			{
				if (0 == wcsncmp(dictName, requiredDictionaries[priority], MAX_STR_LEN))
				{
					break;
				}
			}

			KPTDictStateT &dictState = dictionaryList.dictState[dictIndex];
			dictState.fieldMask  = eKPTDictStateActive | eKPTDictStatePriority;
			if (priority < dictCount)
			{
				// Dictionary required
				dictState.dictActive = eKPTTrue;
				dictState.dictPriority = priority;
				TRACE(KPT_TS("DICTIONARY_SETACTIVELIST Activating %s Priority %d\n"), dictName, priority);
			}
			else
			{
				// Dictionary not required
				// Assign the same priority to all of them and let the API choose the exact numbers
				dictState.dictActive = eKPTFalse;
				dictState.dictPriority = dictionaryList.count - 1;
				TRACE(KPT_TS("DICTIONARY_SETACTIVELIST Deactivating %s\n"), dictName);
			}
		}

		// Activate / deactivate and apply priorities				
		result = (_callKPTFwkRunCmd)(KPTCMD_DICTIONARY_SETSTATES, (intptr_t)dictionaryList.dictState, dictionaryList.count);
		if (KPTRESULT_FAILED(result))
		{
			return result;
		}

		// Free memory
		result = (_callKPTFwkReleaseAlloc)(&dictionaryList);
		return result;
	}

	// Get suggestion configuration options
	KPTResultT FrameworkWrapper::SUGGS_GETCONFIG(void)
	{
		KPTResultT result;
		KPTSuggConfigT config = {0};
		KPTSuggConfigT oldConfig = {0};

		// Get the current configuration 
		config.fieldMask = eKPTSuggsConfigMaskAll;
		result = (_callKPTFwkRunCmd)(KPTCMD_SUGGS_GETCONFIG, (intptr_t)&config, 0);

		return result;
	}

	// Reset the prediction buffer
	KPTResultT FrameworkWrapper::INPUTMGR_RESET(void)
	{
		return (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_RESET, (intptr_t)NULL, 0);
	}

	// Insert a character into the prediction buffer
	KPTResultT FrameworkWrapper::INPUTMGR_INSERTCHAR(KPTUniCharT ch)
	{
		KPTInpMgrInsertCharT insertChar = { 0 };

		insertChar.insertChar = ch;
		return (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_INSERTCHAR, (intptr_t)&insertChar, 0);
	}

	// Insert a string into the prediction buffer
	KPTResultT FrameworkWrapper::INPUTMGR_INSERTSTRING(const KPTUniCharT *str, size_t numChars)
	{
		KPTInpMgrInsertStringT stringToInsert = {0};

		// NumChars excludes NULL
		stringToInsert.insertString = str;
		stringToInsert.length = numChars;
		stringToInsert.ids = NULL;
    
		return (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_INSERTSTRING, (intptr_t)&stringToInsert, 0);
	}

	// Change the logical cursor position
	KPTResultT FrameworkWrapper::INPUTMGR_MOVECURSOR(KPTInpMgrCursorMoveT moveType, int moveAmount)
	{
		return (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_MOVECURSOR, moveType, moveAmount);
	}

	// Remove characters before and/or after the insertion point
	KPTResultT FrameworkWrapper::INPUTMGR_REMOVE(size_t numBefore, size_t numAfter)
	{
		KPTInpMgrRemoveCharsT toRemove;

		toRemove.numBeforeCursor = numBefore;
		toRemove.numAfterCursor = numAfter;
		return (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_REMOVE, (intptr_t)&toRemove, 0);
	}

	// Insert the suggestion with the specified index
	KPTResultT FrameworkWrapper::INPUTMGR_INSERTSUGG(size_t suggestionIndex)
	{
		KPTResultT result;

		KPTInpMgrInsertSuggRequestT suggRequest = {0};
        KPTInpMgrInsertSuggReplyAllocT suggReply = {0};
		if (suggestionIndex < _suggestions.count)
		{
			suggRequest.appendSpace = eKPTFalse;
			suggRequest.suggestionId = _suggestions.suggestions[suggestionIndex].suggestionId;
			suggRequest.suggestionSet = _suggestions.suggestionSet;

			result = (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_INSERTSUGG, (intptr_t)&suggRequest, (intptr_t)&suggReply);

			(_callKPTFwkReleaseAlloc)(&suggReply);
		}
		else
		{
			result = KPTRESULT_MAKE(KPT_SV_ERROR, KPT_COMPONENTID_INVALID, KPT_SC_ERROR);
		}

		return result;
	}

	// Get the current word in the input buffer
	KPTResultT FrameworkWrapper::INPUTMGR_GETCURRWORD(KPTInpMgrCurrentWordT &currentWord)
	{
		KPTResultT result;

		currentWord.composition.fieldMask = eKPTCompositionMaskAll;
		currentWord.fieldMask = eKPTCurrentWordMaskAll;
		result = (_callKPTFwkRunCmd)(KPTCMD_INPUTMGR_GETCURRWORD, (intptr_t)&currentWord, 0);

		// Testing
		//if (KPTRESULT_ISSUCCESS(result))
		//{
		//	TRACE(_T("Prefix:  %s\n"), currentWord.fixedPrefix);
		//	TRACE(_T("Suffix:  %s\n"), currentWord.fixedSuffix);
			//TRACE(_T("Offset:  %d\n"), currentWord.suggestionOffset);
			//TRACE(_T("CompLen: %d\n"), currentWord.composition.compStringLength);
		//}
		//else
		//{
			//TRACE(_T("GETCURRENTWORD error\n"));
		//}

		return result;
	}

	// Get a list of suggestions
	KPTResultT FrameworkWrapper::SUGGS_GETSUGGESTIONS()
	{
		KPTResultT result;
		KPTSuggWordsRequestT suggRequest;

		suggRequest.suggestionTag = 0; // No filtering 
		result = (_callKPTFwkRunCmd)(KPTCMD_SUGGS_GETSUGGESTIONS, 
			(intptr_t)&suggRequest,
			(intptr_t)&_suggestions);

		return result;
	}

	// Return the current suggestions
	const KPTSuggWordsReplyT &FrameworkWrapper::GetCurrentSuggestions()
	{
		return _suggestions;
	}

	// Get the learning options
	KPTResultT FrameworkWrapper::LEARN_GETOPTIONS(uint32_t &options)
	{
		return (_callKPTFwkRunCmd)(KPTCMD_LEARN_GETOPTIONS, (intptr_t)&options, 0);		
	}

	// Set the learning options
	KPTResultT FrameworkWrapper::LEARN_SETOPTIONS(uint32_t options)
	{
		return (_callKPTFwkRunCmd)(KPTCMD_LEARN_SETOPTIONS, (intptr_t)options, NULL);
	}

	// Print out a list
	void FrameworkWrapper::ShowList(KPTDictListAllocT* aList)
	{
		if (0 == aList->count)
		{
			TRACE(KPT_TS("No dictionaries\n"));
		}
		else
		{
			size_t index;
			TRACE(KPT_TS("Name\tVersion\tId\tLoaded\tActive\tPriority\n"));
			for (index = 0; index < aList->count; ++index)
			{
				TRACE(aList->dictInfo[index].dictDisplayName);
				TRACE(KPT_TS("\t%d\t0x%X\t%d\t%d\t%d\n"),
					aList->dictInfo[index].dictVersion,
					aList->dictState[index].componentId,
					aList->dictState[index].dictLoaded,
					aList->dictState[index].dictActive,
					aList->dictState[index].dictPriority);
			}
			TRACE(KPT_TS("Done.\n"));
		}
	}

