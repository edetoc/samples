//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#pragma once

#include <credentialprovider.h>
#include <windows.h>
#include <strsafe.h>

#include "CSampleCredential.h"
#include "helpers.h"

#include <fstream>
#include <iostream>
using namespace std;

class CSampleProvider : public ICredentialProvider
{
  public:
    // IUnknown
    IFACEMETHODIMP_(ULONG) AddRef()
    {
        return ++_cRef;
    }
    
    IFACEMETHODIMP_(ULONG) Release()
    {
        LONG cRef = --_cRef;
        if (!cRef)
        {
            delete this;
        }
        return cRef;
    }

    IFACEMETHODIMP QueryInterface(__in REFIID riid, __deref_out void** ppv)
    {
        static const QITAB qit[] =
        {
            QITABENT(CSampleProvider, ICredentialProvider), // IID_ICredentialProvider
            {0},
        };
        return QISearch(this, qit, riid, ppv);
    }

  public:
    IFACEMETHODIMP SetUsageScenario(__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, __in DWORD dwFlags);
    IFACEMETHODIMP SetSerialization(__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs);

    IFACEMETHODIMP Advise(__in ICredentialProviderEvents* pcpe, __in UINT_PTR upAdviseContext);
    IFACEMETHODIMP UnAdvise();

    IFACEMETHODIMP GetFieldDescriptorCount(__out DWORD* pdwCount);
    IFACEMETHODIMP GetFieldDescriptorAt(__in DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd);

    IFACEMETHODIMP GetCredentialCount(__out DWORD* pdwCount,
                                      __out_range(<,*pdwCount) DWORD* pdwDefault,
                                      __out BOOL* pbAutoLogonWithDefault);
    IFACEMETHODIMP GetCredentialAt(__in DWORD dwIndex, 
                                   __deref_out ICredentialProviderCredential** ppcpc);

    friend HRESULT CSample_CreateInstance(__in REFIID riid, __deref_out void** ppv);

  protected:
    CSampleProvider();
    __override ~CSampleProvider();
    
  private:
      void _CleanUpAllCredentials();
    
private:
    LONG                _cRef;
    CSampleCredential   **_rgpCredentials;          // Pointers to the credentials which will be enumerated by this 
                                                    // Provider.

    ICredentialProvider *_pWrappedProvider;         // Our wrapped provider.
    DWORD               _dwCredentialCount;         // The number of credentials provided by our wrapped provider.
    DWORD               _dwWrappedDescriptorCount;  // The number of fields on each tile of our wrapped provider's 
                                                    // credentials.
    bool                _bEnumeratedSetSerialization;

	ofstream			outfile;					// edetoc: logging for demo purpose
};
