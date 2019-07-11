// WinBioIdentifyUserSample.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <stdio.h>
#include <conio.h>
#include <WinBio.h>

#pragma comment (lib, "Winbio.lib")

int _tmain(int argc, _TCHAR* argv[])
{

	HRESULT hr = S_OK;
	WINBIO_IDENTITY identity = { 0 };
	WINBIO_SESSION_HANDLE sessionHandle = NULL;
	WINBIO_UNIT_ID unitId = 0;

	PWINBIO_BIOMETRIC_SUBTYPE subFactorArray = NULL;
	WINBIO_BIOMETRIC_SUBTYPE SubFactor = 0;
	SIZE_T subFactorCount = 0;
	WINBIO_REJECT_DETAIL rejectDetail = 0;
	WINBIO_BIOMETRIC_SUBTYPE subFactor = WINBIO_SUBTYPE_NO_INFORMATION;

	WCHAR   szUsername[MAX_PATH] = { 0 };
	DWORD   cchUsername = ARRAYSIZE(szUsername);
	WCHAR   szDomain[MAX_PATH] = { 0 };
	DWORD   cchDomain = ARRAYSIZE(szDomain);
	DWORD   dwResult;

	// Connect to the system pool. 
	hr = WinBioOpenSession(
		WINBIO_TYPE_FINGERPRINT,    // Service provider
		WINBIO_POOL_SYSTEM,         // Pool type
		WINBIO_FLAG_DEFAULT,        // Configuration and access
		NULL,                       // Array of biometric unit IDs
		0,                          // Count of biometric unit IDs
		NULL,                       // Database ID
		&sessionHandle              // [out] Session handle
		);

	if (FAILED(hr))
	{
		wprintf_s(L"\n WinBioOpenSession failed. hr = 0x%x\n", hr);
		goto e_Exit;
	}

	// Locate the biometric sensor and retrieve a WINBIO_IDENTITY object.
	wprintf_s(L"\n Calling WinBioIdentify - Swipe finger on sensor...\n");
	hr = WinBioIdentify(
		sessionHandle,              // Session handle
		&unitId,                    // Biometric unit ID
		&identity,                  // User SID
		&subFactor,                 // Finger sub factor
		&rejectDetail               // Rejection information
		);

	wprintf_s(L"\n Swipe processed - Unit ID: %d\n", unitId);
	if (FAILED(hr))
	{
		if (hr == WINBIO_E_UNKNOWN_ID)
		{
			wprintf_s(L"\n Unknown identity.\n");
		}
		else if (hr == WINBIO_E_BAD_CAPTURE)
		{
			wprintf_s(L"\n Bad capture; reason: %d\n", rejectDetail);
		}
		else
		{
			wprintf_s(L"\n WinBioEnumBiometricUnits failed. hr = 0x%x\n", hr);
		}
		goto e_Exit;
	}

	// let's do sid to name now...

	SID_NAME_USE    SidUse;
	DWORD           cchTmpUsername = cchUsername;
	DWORD           cchTmpDomain = cchDomain;

	if (!LookupAccountSidW(
		NULL,										// Local computer
		identity.Value.AccountSid.Data,             // Security identifier of user who just swiped his fingers
		szUsername,									// User name
		&cchTmpUsername,							// Size of user name
		szDomain,									// Domain name
		&cchTmpDomain,								// Size of domain name
		&SidUse))									// Account type
	{
		dwResult = GetLastError();
		hr = HRESULT_FROM_WIN32(dwResult);
		wprintf_s(L"\n LookupAccountSidLocalW failed: hr = 0x%x\n", hr);
		goto e_Exit;
	}

	wprintf_s(L"\n username: %s\n", szUsername);
	wprintf_s(L"\n domain: %s\n", szDomain);

e_Exit:
	if (subFactorArray != NULL)
	{
		WinBioFree(subFactorArray);
		subFactorArray = NULL;
	}

	if (sessionHandle != NULL)
	{
		WinBioCloseSession(sessionHandle);
		sessionHandle = NULL;
	}

	wprintf_s(L"\n Press any key to exit...");
	_getch();

	return hr;

}

