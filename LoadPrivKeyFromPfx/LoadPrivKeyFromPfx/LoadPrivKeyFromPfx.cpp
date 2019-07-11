// LoadPrivKeyFromPfx.cpp : Defines the entry point for the console application.
//
//

#include "stdafx.h"
#include <assert.h>  
#include <memory>
#include <windows.h>
#include <strsafe.h>
#include <Wincrypt.h>
#include <Wintrust.h>
#include <mssip.h>
#include <io.h>
#include <iostream>
#include <fstream>

void MyGetLastError(LPTSTR);

int _tmain(int argc, _TCHAR* argv[])
{
	HCRYPTPROV hCryptProv;
	DWORD dwKeySpec;
	BOOL fCallerFreeProvOrNCryptKey;

	FILE* pfxFile;
	errno_t err;

	printf("Press a key to start.");
	getchar();

	// Read the content of the PFX file
	// Note: PFX path is provided in argv[1]
	
	_tprintf(_T("Attempt to load file %s\n"), argv[1]);

	err = _tfopen_s(&pfxFile, argv[1], _T("rb"));

	if (err == 0)
	{
		_tprintf(_T("The file %s was opened\n"),argv[1]);
	}
	else
	{
		_tprintf(_T("The file %s was not opened\n"), argv[1]);
		return -1;
	}

	long pfxLength = _filelength(_fileno(pfxFile));
	LPBYTE pbPfxData = (LPBYTE)LocalAlloc(0, pfxLength);
	fread(pbPfxData, 1, pfxLength, pfxFile);
	fclose(pfxFile);

	// Decrypt 	the content of the PFX file 
	CRYPT_DATA_BLOB pfxBlob;
	pfxBlob.cbData = pfxLength;
	pfxBlob.pbData = pbPfxData;

	HCERTSTORE hPfxStore = PFXImportCertStore(&pfxBlob, NULL, PKCS12_NO_PERSIST_KEY ); // PKCS12_NO_PERSIST_KEY : prevents the key from being persisted to disk
	
	PCCERT_CONTEXT hContext = CertFindCertificateInStore(hPfxStore, X509_ASN_ENCODING | PKCS_7_ASN_ENCODING, 0, CERT_FIND_ANY, nullptr, nullptr);
	if (hContext == NULL)
	{
		MyGetLastError(TEXT("CertFindCertificateInStore"));
	}

	if (CryptAcquireCertificatePrivateKey(hContext, CRYPT_ACQUIRE_CACHE_FLAG, nullptr, &hCryptProv, &dwKeySpec, &fCallerFreeProvOrNCryptKey))
	{
		printf("CryptAcquireCertificatePrivateKey: Success");
	}
	else
	{
		MyGetLastError(TEXT("CryptAcquireCertificatePrivateKey"));
	}

	return 0;
}

void MyGetLastError(LPTSTR lpszFunction)
{
	// Retrieve the system error message for the last-error code

	LPVOID lpMsgBuf;
	LPVOID lpDisplayBuf;
	DWORD dw = GetLastError();

	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0, NULL);

	// Display the error message and exit the process

	lpDisplayBuf = (LPVOID)LocalAlloc(LMEM_ZEROINIT,
		(lstrlen((LPCTSTR)lpMsgBuf) + lstrlen((LPCTSTR)lpszFunction) + 40) * sizeof(TCHAR));
	StringCchPrintf((LPTSTR)lpDisplayBuf,
		LocalSize(lpDisplayBuf) / sizeof(TCHAR),
		TEXT("%s failed with error %d: %s"),
		lpszFunction, dw, lpMsgBuf);
	MessageBox(NULL, (LPCTSTR)lpDisplayBuf, TEXT("Error"), MB_OK);

	LocalFree(lpMsgBuf);
	LocalFree(lpDisplayBuf);
	ExitProcess(dw);
}

