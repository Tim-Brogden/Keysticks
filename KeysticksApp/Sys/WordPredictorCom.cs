/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Keysticks.Sys
{
    [ComImport]
    [Guid("F9692587-91D6-443D-8040-EAFE087847F3")]
    public class WordPredictorCom
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("68C3F820-D0A2-4CC1-87CD-BB5F1BAB974B")]
    public interface IWordPredictorCom
    {
        [DispId(1)]
        void Create(string bstrBasePath);
        [DispId(2)]
        void Destroy();
        [DispId(3)]
        int ProcessRequest(
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] requestMeta, 
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] string[] requestData, 
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] ref string[] responseData);
        //[DispId(4)]
        //int TestIn([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] string[] requestData);
        //[DispId(5)]
        //void TestOut([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] ref string[] requestData);
    }
}