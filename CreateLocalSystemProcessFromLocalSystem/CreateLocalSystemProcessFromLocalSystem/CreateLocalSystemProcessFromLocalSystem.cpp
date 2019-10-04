

#include "stdafx.h"
#include<Windows.h>
#include<WtsApi32.h>
#include<UserEnv.h>
#include<strsafe.h>


void TestFunc(void)
{
	DWORD  dwActiveSession = WTSGetActiveConsoleSessionId();
	HANDLE hToken;
	HANDLE DupToken;

	//
	// enumerate sessions
	//
	PWTS_SESSION_INFO pwsi = NULL;
	DWORD dwCount;

	if (!WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &pwsi, &dwCount))
	{
		wprintf(L"WTSEnumerateSessions failed with %u\n", GetLastError());
		return;
	}
	else
	{
		for (DWORD i = 0; i < dwCount; i++)
		{
			wprintf(L"Session ID: %u, State: %u\n", pwsi[i].SessionId, pwsi[i].State);

			if (pwsi[i].State == WTSActive)
			{
				if (pwsi[i].SessionId == dwActiveSession)
				{
					//
					// get system token
					//
					if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, &hToken))
					{
						wprintf(L"OpenProcessToken failed with %u\n", GetLastError());
						return;
					}

					// Clone the primary system token					
					if (!DuplicateTokenEx(hToken, TOKEN_ALL_ACCESS, NULL, SecurityAnonymous, TokenPrimary, &DupToken))
					{
						wprintf(L"DuplicateTokenEx failed with %u\n", GetLastError());
						return;
					}
					
					if (!SetTokenInformation( DupToken, TokenSessionId, (LPVOID)&dwActiveSession, sizeof(dwActiveSession)))				
					{
						wprintf(L"SetTokenInformation failed with %u\n", GetLastError());
						return;
					}

					WCHAR               szCommandLine[256] = L"c:\\windows\\system32\\cmd.exe";
					STARTUPINFO         si = { 0 };
					PROCESS_INFORMATION pi;

					si.cb = sizeof(STARTUPINFO);
					si.lpDesktop = L"winsta0\\default";
					si.lpTitle = L"FROM SERVICE";

					if (!CreateProcessAsUser( DupToken, NULL, szCommandLine, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, NULL, &si, &pi))
					{
						wprintf(L"CreateProcessAsUser failed with %u\n", GetLastError());
						return;
					}

					//
					// CLEANUP
					//
					CloseHandle(hToken);
					CloseHandle(DupToken);
					CloseHandle(pi.hProcess);
					CloseHandle(pi.hThread);
				}
			}
		}

		WTSFreeMemory(pwsi);
	}
}


void wmain(void)
{
	TestFunc();
}

