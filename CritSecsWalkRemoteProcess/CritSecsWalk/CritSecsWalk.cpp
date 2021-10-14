// https://docs.microsoft.com/en-us/archive/msdn-magazine/2003/december/break-free-of-code-deadlocks-in-critical-sections-under-windows
// https://docs.microsoft.com/en-us/windows/win32/sync/using-critical-section-objects
// https://www.codeproject.com/Articles/800404/Understanding-LIST-ENTRY-Lists-and-Its-Importance
// https://docs.microsoft.com/en-us/windows/win32/debug/retrieving-symbol-information-by-address
// https://docs.microsoft.com/en-us/windows/win32/debug/using-symsrv#installation


#include <Windows.h>
#include <stdio.h>
#include <DbgHelp.h>


DWORD g_pid;	// target process

bool ProcessCommandLine(int argc, char* argv[]);

int main(int argc, char* argv[])
{

	if (!ProcessCommandLine(argc, argv))
		return FALSE;

	printf("\nFinding critical sections for process %d ...", g_pid);

	// Open a process for ReadProcessMemory and DbgHelp.
	HANDLE InterestingProcess = OpenProcess( PROCESS_ALL_ACCESS, false, g_pid);
	if (InterestingProcess == NULL)
	{
		printf("\nUnable to open the process");
		return -1;
	}

	// Initialize symbol handler
	SymSetOptions(SYMOPT_UNDNAME | SYMOPT_DEFERRED_LOADS | SYMOPT_DEBUG);

	printf("\nInitializing symbol engine");

	// Note: for symsrv.dll to be loaded successully it must be present in same directory than dbghelp.dll used
	if (!SymInitialize(InterestingProcess,	NULL /* UserSearchPath */, TRUE /* fInvadeProcess */))	
	{
	
		printf("\nInitializing symbol engine KO. SymInitialize returned error : %d", GetLastError());
		return FALSE;
	}

	printf("\nInitializing symbol engine OK");

	CHAR searchPath[MAX_PATH + 1];

	if (!SymGetSearchPath(InterestingProcess, searchPath, MAX_PATH + 1))
	{
		printf("\nSymGetSearchPath returned error : %d", GetLastError());
		return FALSE;
	}

	printf("\nSymbol search path: %s", searchPath);

	ULONG64 buffer[(sizeof(SYMBOL_INFO) +
		MAX_SYM_NAME * sizeof(TCHAR) +
		sizeof(ULONG64) - 1) /
		sizeof(ULONG64)];

	PSYMBOL_INFO pSymbol = (PSYMBOL_INFO)buffer;
	pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
	pSymbol->MaxNameLen = MAX_SYM_NAME;

	if (SymFromName(InterestingProcess, "ntdll!RtlCriticalSectionList", pSymbol))
	{
		printf("\nAddress of %s: %p\n", pSymbol->Name, (void*)pSymbol->Address);    
		
	}
	else
	{

		DWORD error = GetLastError();
		printf("\nSymFromName returned error : %d", error);
		return FALSE;
	}


	/*
	* dev note:
	
	0:002 > dt ntdll!_RTL_CRITICAL_SECTION_DEBUG
		+ 0x000 Type             : Uint2B
		+ 0x002 CreatorBackTraceIndex : Uint2B
		+ 0x008 CriticalSection : Ptr64 _RTL_CRITICAL_SECTION
		+ 0x010 ProcessLocksList : _LIST_ENTRY						   < ===  ntdll!RtlCriticalSectionList
		+ 0x020 EntryCount : Uint4B
		+ 0x024 ContentionCount : Uint4B
		+ 0x028 Flags : Uint4B
		+ 0x02c CreatorBackTraceIndexHigh : Uint2B
		+ 0x02e SpareUSHORT : Uint2B

	
	*/
	
	LIST_ENTRY* pCriticalSectionList = (LIST_ENTRY*)(pSymbol->Address); // points on first ProcessLocksList


	// Walk the list of critical sections

	LIST_ENTRY* pCSListHead = pCriticalSectionList;
	LIST_ENTRY* pCSCurrentNode = pCSListHead->Flink;

	int i = 1;

	printf("\nStart enumerating the critical sections\n");

	while (pCSCurrentNode != pCSListHead)
	{

		// 1. Read the Debug structure

		_RTL_CRITICAL_SECTION_DEBUG DebugInfoEntry = { 0 };
		SIZE_T ulBytesRead = 0;

		if (!ReadProcessMemory(InterestingProcess,
			(LPCVOID)((ULONG_PTR)pCSCurrentNode - offsetof(_RTL_CRITICAL_SECTION_DEBUG, ProcessLocksList)), // 0x10
			&DebugInfoEntry,
			sizeof(RTL_CRITICAL_SECTION_DEBUG),
			&ulBytesRead))
		{
			printf("\nCould not read _RTL_CRITICAL_SECTION_DEBUG. Error = %X\n", GetLastError());
			return FALSE;
		}


		// 2. Read the Critical structure nested in the DebugInfoEntry

		_RTL_CRITICAL_SECTION CritsecEntry = { 0 };

		if (!ReadProcessMemory(InterestingProcess,
			(LPCVOID)(DebugInfoEntry.CriticalSection), 
			&CritsecEntry,
			sizeof(_RTL_CRITICAL_SECTION),
			&ulBytesRead))
		{
			printf("\nCould not read _RTL_CRITICAL_SECTION. Error = %X\n", GetLastError());
			return FALSE;
		}

		// 3. Attempt to resolve the symbol name of the critical section address

		DWORD64 dwDisplacement = 0;
		ULONG_PTR lpAddress = (ULONG_PTR)DebugInfoEntry.CriticalSection;

		char buffer[sizeof(SYMBOL_INFO) + MAX_SYM_NAME * sizeof(TCHAR)];

		PSYMBOL_INFO pSymbol = (PSYMBOL_INFO)buffer;
		pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
		pSymbol->MaxNameLen = MAX_SYM_NAME;

		char symName[MAX_SYM_NAME + 1];

		if (SymFromAddr(InterestingProcess, (DWORD64)lpAddress, &dwDisplacement, pSymbol))
		{
			strcpy_s(symName, sizeof symName, (char*)&pSymbol->Name);
		}
		else
		{
			strcpy_s(symName, sizeof symName, "");
		}


		// 4. Print Critical Section details

		printf("\n****** Critsec %d  ********", i);
		printf("\nCritsec %s at %p", strlen(symName) > 0 ? symName : "(symbol not found)", DebugInfoEntry.CriticalSection);
		printf("\nOwning thread %p\n", CritsecEntry.OwningThread);

		// 5. Move to next critical section 

		pCSCurrentNode = DebugInfoEntry.ProcessLocksList.Flink;

		i++;

	} 

	printf("\nDone with enumerating the critical sections");

	printf("\n\nPress Enter to exit");
	int c = getchar();

	// do some cleanup
	SymCleanup(InterestingProcess);
	CloseHandle(InterestingProcess);

	return TRUE;
}

bool ProcessCommandLine(int argc, char* argv[])
{
	if (argc < 2)
	{
		
		printf("\nSyntax: %s <PID>\n", argv[0]);
		
		return false;
	}

	g_pid = atoi(argv[1]);
	if (g_pid == 0)
	{
		printf("\nInvalid PID specified.\n");
		return false;
	}

	
	return true;
}

