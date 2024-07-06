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

class CWordPredictorModule : public ATL::CAtlDllModuleT< CWordPredictorModule >
{
public :
	DECLARE_LIBID(LIBID_WordPredictorLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_WORDPREDICTOR, "{7F4F5A07-5AFF-499F-8EAD-D8A46E14CC52}")
};

extern class CWordPredictorModule _AtlModule;
