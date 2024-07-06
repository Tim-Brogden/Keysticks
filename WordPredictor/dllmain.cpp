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
#include "resource.h"
#include "WordPredictor_i.h"
#include "dllmain.h"

CWordPredictorModule _AtlModule;

// DLL Entry Point
extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	hInstance;
	return _AtlModule.DllMain(dwReason, lpReserved); 
}
