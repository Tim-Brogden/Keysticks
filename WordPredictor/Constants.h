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


	#define REQUEST_RESET_INPUT 10
	#define REQUEST_INSERT_STRING 11
	#define REQUEST_MOVE_CURSOR 12
	#define REQUEST_REMOVE_CHARS 13
	#define REQUEST_INSERT_SUGGESTION 14
	#define REQUEST_CONFIGURE_LEARNING 15
	#define REQUEST_SET_CURSOR 16
	#define REQUEST_GET_SUGGESTIONS 17
	#define REQUEST_INSTALL_PACKAGES 18
	#define REQUEST_UNINSTALL_PACKAGES 19
	#define REQUEST_SET_ACTIVE_DICTIONARIES 20

	#define RESPONSE_ERROR_UNRECOGNISED_MSG_TYPE 200
	#define RESPONSE_ERROR_BUFFER_OVERFLOW 201
	#define RESPONSE_ERROR_RESET 210
	#define RESPONSE_ERROR_INSERT_STRING 211
	#define RESPONSE_ERROR_MOVE_CURSOR 212
	#define RESPONSE_ERROR_REMOVE_CHARS 213
	#define RESPONSE_ERROR_INSERT_SUGGESTION 214
	#define RESPONSE_ERROR_CONFIGURE_LEARNING 215
	#define RESPONSE_ERROR_SET_CURSOR 216
	#define RESPONSE_ERROR_GET_SUGGESTIONS 217
	#define RESPONSE_ERROR_INSTALL_PACKAGES 218
	#define RESPONSE_ERROR_UNINSTALL_PACKAGES 219
	#define RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES 220

	#define MAX_STR_LEN 1024	
	#define MAX_DICTIONARIES 100
	#define DEFAULT_RELATIVE_BASE_PATH L"..\\data\\base"
