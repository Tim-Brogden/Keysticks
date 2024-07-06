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

#include <crtdbg.h>
#include <stdarg.h>
#include <stdio.h>
#include <string.h>

#ifdef _DEBUG

	#define TRACEMAXSTRING  1024

	extern wchar_t debug_szBuffer[TRACEMAXSTRING];

	// Debugging utility methods
	inline void TRACE(const wchar_t* format,...)
	{
		va_list args;
		va_start(args,format);
		int nBuf;
		nBuf = _vsnwprintf_s(debug_szBuffer,
									TRACEMAXSTRING,
									format,
									args);
		va_end(args);

		_RPTW0(_CRT_WARN, debug_szBuffer);
	}
	#define TRACEF _snwprintf(debug_szBuffer, TRACEMAXSTRING,"%s(%d): ", \
									&wcsrchr(__FILE__,'\\')[1], __LINE__); \
									_RPT0(_CRT_WARN, debug_szBuffer); \
									TRACE
#else

	// Remove for release mode
	#define TRACE  __noop
	#define TRACEF __noop

#endif
