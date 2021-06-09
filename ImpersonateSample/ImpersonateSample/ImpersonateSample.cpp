//
// this program should be executed from a cmd running as local system (psexec -s -i cmd.exe)
// It takes a PID as argument. this can be the pid of Notepad.exe launched by the user prior to launch this sample.
// this PID is used to obtain the token of Notepad.exe process.
//
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <Lmcons.h>

#define CheckAndLocalFree(ptr) \
            if (ptr != NULL) \
            { \
               LocalFree(ptr); \
               ptr = NULL; \
            }

LPVOID RetrieveTokenInformationClass(
    HANDLE hToken,
    TOKEN_INFORMATION_CLASS InfoClass,
    LPDWORD dwSize);


LPVOID RetrieveTokenInformationClass(
    HANDLE hToken,
    TOKEN_INFORMATION_CLASS InfoClass,
    LPDWORD lpdwSize)
{
    LPVOID pInfo = NULL;
    BOOL fSuccess = FALSE;

    __try
    {
        *lpdwSize = 0;

        //
        // Determine size of buffer needed
        //

        GetTokenInformation(
            hToken,
            InfoClass,
            NULL,
            *lpdwSize, lpdwSize);
        if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
        {
            wprintf(L"GetTokenInformation failed with %d\n", GetLastError());
            __leave;
        }

        //
        // Allocate a buffer for getting token information
        //
        pInfo = LocalAlloc(LPTR, *lpdwSize);
        if (pInfo == NULL)
        {
            wprintf(L"LocalAlloc failed with %d\n", GetLastError());
            __leave;
        }

        if (!GetTokenInformation(
            hToken,
            InfoClass,
            pInfo,
            *lpdwSize, lpdwSize))
        {
            wprintf(L"GetTokenInformation failed with %d\n", GetLastError());
            __leave;
        }

        fSuccess = TRUE;
    }
    __finally
    {
        // Free pDomainAndUserName only if failed.
        // Otherwise, the caller has to free after use
        if (fSuccess == FALSE)
        {
            CheckAndLocalFree(pInfo);
        }
    }

    return pInfo;
}


int wmain(int argc, WCHAR** argv)
{

    HANDLE processHandle;
    HANDLE hToken;

    // Grab PID from command line argument. typically the PID of notepad.exe launched by the user
    WCHAR* pid_c = argv[1];
    DWORD PID = _wtoi(pid_c);

    int i = 0;

    while (true)
    {

        // this impersonation is done every second 
        Sleep(1000);
        printf("\n");
        printf("=== iteration %d ===\n",i);
        
        processHandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, 
                                    true, 
                                    PID);   

        if (processHandle == NULL)
        {
            printf("[-] OpenProcess() Error: %i\n", GetLastError());
            exit(-1);
        }
        else
        {
            printf("[+] OpenProcess() success!\n");

        }

        // Open a handle to the access token for the
        // calling process that is the currently login access token

        if (!OpenProcessToken(processHandle, TOKEN_ALL_ACCESS, &hToken))  // TOKEN_ALL_ACCESS
        {
            wprintf(L"OpenProcessToken() - Getting the handle to access token failed, error % u\n", GetLastError());
            exit(-1);
        }
        else
            wprintf(L"OpenProcessToken() - Got the handle to access token!\n");

        CloseHandle(processHandle);

        // check if it is a Primary or Impersonation token

        TOKEN_STATISTICS* pStatistics = NULL;
        DWORD dwSize = 0;

        pStatistics = (TOKEN_STATISTICS*)RetrieveTokenInformationClass(hToken, TokenStatistics, &dwSize);
        if (pStatistics == NULL)
        {
            return FALSE;
        }

        if (pStatistics->TokenType == TokenPrimary)
            wprintf(L"Token type is PRIMARY\n");
        else
            wprintf(L"Token type is IMPERSONATION\n");

        CheckAndLocalFree(pStatistics);


        // Lets the calling process impersonate the security context of the logged-on user.

        if (ImpersonateLoggedOnUser(hToken))
        {
            printf("[+] ImpersonatedLoggedOnUser() success!\n");

        }
        else
        {
            wprintf(L"ImpersonateLoggedOnUser() failed, error % u.\n", GetLastError());
            exit(-1);
        }

        // Run some under impersonated user...
        
        wchar_t userbuf[UNLEN];
        DWORD usersize = UNLEN;
        GetUserName(userbuf, &usersize);
        printf("[+] Current user is: %ls\n", userbuf);  // should be the impersonated logged on user


         // Close the handle

        if (CloseHandle(hToken))

            wprintf(L"Handle to an access token was closed.\n");

        else

            wprintf(L"Failed to close the hToken handle!error % u\n", GetLastError());

        // Terminates the impersonation of a client.

        if (RevertToSelf())

            wprintf(L"Impersonation was terminated.\n");

        i++;
    }
    
    return 0;

}