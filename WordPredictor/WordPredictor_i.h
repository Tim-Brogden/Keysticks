

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.01.0622 */
/* at Tue Jan 19 03:14:07 2038
 */
/* Compiler settings for WordPredictor.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.01.0622 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */



/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __WordPredictor_i_h__
#define __WordPredictor_i_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IWordPredictorCom_FWD_DEFINED__
#define __IWordPredictorCom_FWD_DEFINED__
typedef interface IWordPredictorCom IWordPredictorCom;

#endif 	/* __IWordPredictorCom_FWD_DEFINED__ */


#ifndef __WordPredictorCom_FWD_DEFINED__
#define __WordPredictorCom_FWD_DEFINED__

#ifdef __cplusplus
typedef class WordPredictorCom WordPredictorCom;
#else
typedef struct WordPredictorCom WordPredictorCom;
#endif /* __cplusplus */

#endif 	/* __WordPredictorCom_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IWordPredictorCom_INTERFACE_DEFINED__
#define __IWordPredictorCom_INTERFACE_DEFINED__

/* interface IWordPredictorCom */
/* [unique][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IWordPredictorCom;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("68C3F820-D0A2-4CC1-87CD-BB5F1BAB974B")
    IWordPredictorCom : public IDispatch
    {
    public:
        virtual /* [id] */ HRESULT STDMETHODCALLTYPE Create( 
            /* [in] */ BSTR bstrBasePath) = 0;
        
        virtual /* [id] */ HRESULT STDMETHODCALLTYPE Destroy( void) = 0;
        
        virtual /* [id] */ HRESULT STDMETHODCALLTYPE ProcessRequest( 
            /* [in] */ SAFEARRAY * requestMeta,
            /* [in] */ SAFEARRAY * requestData,
            /* [out] */ SAFEARRAY * *responseData,
            /* [retval][out] */ int *responseCode) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IWordPredictorComVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IWordPredictorCom * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IWordPredictorCom * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IWordPredictorCom * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IWordPredictorCom * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IWordPredictorCom * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IWordPredictorCom * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IWordPredictorCom * This,
            /* [annotation][in] */ 
            _In_  DISPID dispIdMember,
            /* [annotation][in] */ 
            _In_  REFIID riid,
            /* [annotation][in] */ 
            _In_  LCID lcid,
            /* [annotation][in] */ 
            _In_  WORD wFlags,
            /* [annotation][out][in] */ 
            _In_  DISPPARAMS *pDispParams,
            /* [annotation][out] */ 
            _Out_opt_  VARIANT *pVarResult,
            /* [annotation][out] */ 
            _Out_opt_  EXCEPINFO *pExcepInfo,
            /* [annotation][out] */ 
            _Out_opt_  UINT *puArgErr);
        
        /* [id] */ HRESULT ( STDMETHODCALLTYPE *Create )( 
            IWordPredictorCom * This,
            /* [in] */ BSTR bstrBasePath);
        
        /* [id] */ HRESULT ( STDMETHODCALLTYPE *Destroy )( 
            IWordPredictorCom * This);
        
        /* [id] */ HRESULT ( STDMETHODCALLTYPE *ProcessRequest )( 
            IWordPredictorCom * This,
            /* [in] */ SAFEARRAY * requestMeta,
            /* [in] */ SAFEARRAY * requestData,
            /* [out] */ SAFEARRAY * *responseData,
            /* [retval][out] */ int *responseCode);
        
        END_INTERFACE
    } IWordPredictorComVtbl;

    interface IWordPredictorCom
    {
        CONST_VTBL struct IWordPredictorComVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IWordPredictorCom_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IWordPredictorCom_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IWordPredictorCom_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IWordPredictorCom_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IWordPredictorCom_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IWordPredictorCom_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IWordPredictorCom_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IWordPredictorCom_Create(This,bstrBasePath)	\
    ( (This)->lpVtbl -> Create(This,bstrBasePath) ) 

#define IWordPredictorCom_Destroy(This)	\
    ( (This)->lpVtbl -> Destroy(This) ) 

#define IWordPredictorCom_ProcessRequest(This,requestMeta,requestData,responseData,responseCode)	\
    ( (This)->lpVtbl -> ProcessRequest(This,requestMeta,requestData,responseData,responseCode) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IWordPredictorCom_INTERFACE_DEFINED__ */



#ifndef __WordPredictorLib_LIBRARY_DEFINED__
#define __WordPredictorLib_LIBRARY_DEFINED__

/* library WordPredictorLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_WordPredictorLib;

EXTERN_C const CLSID CLSID_WordPredictorCom;

#ifdef __cplusplus

class DECLSPEC_UUID("F9692587-91D6-443D-8040-EAFE087847F3")
WordPredictorCom;
#endif
#endif /* __WordPredictorLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

unsigned long             __RPC_USER  LPSAFEARRAY_UserSize(     unsigned long *, unsigned long            , LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserMarshal(  unsigned long *, unsigned char *, LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserUnmarshal(unsigned long *, unsigned char *, LPSAFEARRAY * ); 
void                      __RPC_USER  LPSAFEARRAY_UserFree(     unsigned long *, LPSAFEARRAY * ); 

unsigned long             __RPC_USER  BSTR_UserSize64(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal64(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal64(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree64(     unsigned long *, BSTR * ); 

unsigned long             __RPC_USER  LPSAFEARRAY_UserSize64(     unsigned long *, unsigned long            , LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserMarshal64(  unsigned long *, unsigned char *, LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserUnmarshal64(unsigned long *, unsigned char *, LPSAFEARRAY * ); 
void                      __RPC_USER  LPSAFEARRAY_UserFree64(     unsigned long *, LPSAFEARRAY * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


