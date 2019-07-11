// CPAUFromLocalSystem.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include<Windows.h>
#include<WtsApi32.h>
#include<UserEnv.h>
#include<strsafe.h>


void ErrorExit(LPTSTR lpszFunction)
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


int main()
{

	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	DWORD dwSessionID = 0;
	HANDLE hUserToken = NULL;
	LPVOID lpEnv = NULL;

	printf("Press a key to continue");
	getchar();

	dwSessionID = WTSGetActiveConsoleSessionId();
	if (dwSessionID == 0xFFFFFFFF)
	{
		printf("No session attached to the physical console");
		goto cleanup;
	}

	if (!WTSQueryUserToken(dwSessionID, &hUserToken))
	{
		ErrorExit(TEXT("WTSQueryUserToken"));
		goto cleanup;
	}


	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));
	si.lpDesktop = L"winsta0\\default";

	if (!CreateEnvironmentBlock(&lpEnv, hUserToken, FALSE))
	{
		ErrorExit(TEXT("CreateEnvironmentBlock"));
		goto cleanup;
	}

	if (!CreateProcessAsUser(
		hUserToken, 
		L"C:\\WINDOWS\\SYSTEM32\\NOTEPAD.EXE",
	    NULL, //L"C:\\WINDOWS\\SYSTEM32\\NOTEPAD.EXE", 
		NULL, 
		NULL, 
		FALSE, 
		CREATE_UNICODE_ENVIRONMENT, 
		lpEnv, 
		NULL, 
		&si, 
		&pi))
	{
		ErrorExit(TEXT("CreateProcessAsUser"));
		goto cleanup;
	}
	else
	{
		printf("CreateProcessAsUser suceeded!");
	}


cleanup:

	if (hUserToken)
		CloseHandle(hUserToken);

	if (lpEnv)
		DestroyEnvironmentBlock(lpEnv);


	return 0;
}

