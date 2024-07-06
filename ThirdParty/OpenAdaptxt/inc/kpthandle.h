/******************************************************************************
 *
 * Copyright 2011 KeyPoint Technologies (UK) Ltd.   
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html

 *           
 * Contributors: 
 * KeyPoint Technologies (UK) Ltd - Initial API and implementation
 *
 *****************************************************************************/

 

/******************************************************************************
 * COPYRIGHT (c) 2008 KeyPoint Technologies (UK) Ltd. All rights reserved.
 *
 * The copyright to the computer program(s) herein is the property of KeyPoint
 * Technologies (UK) Ltd. The program(s) may be used and/or copied only with 
 * the written permission from KeyPoint Technologies (UK) Ltd or in accordance
 * with the terms and conditions stipulated in the agreement/contract under 
 * which the program(s) have been supplied.
 */
/**
 * @file    kpthandle.h
 *
 * @brief   Support for interface definitions
 *
 * @details
 *
 *****************************************************************************/
#if defined (_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

#ifndef H_KPTHANDLE_H
#define H_KPTHANDLE_H

/** Macro for declaring opaque pointers with a private implementation of "name" */
#define DECLARE_KPTHANDLE(name) \
	typedef struct name name; \
	struct name;

#endif /* #ifndef H_KPTHANDLE_H */
