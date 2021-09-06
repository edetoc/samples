// https://docs.microsoft.com/en-us/archive/msdn-magazine/2003/december/break-free-of-code-deadlocks-in-critical-sections-under-windows
// https://docs.microsoft.com/en-us/windows/win32/sync/using-critical-section-objects
// https://www.codeproject.com/Articles/800404/Understanding-LIST-ENTRY-Lists-and-Its-Importance
// https://docs.microsoft.com/en-us/windows/win32/debug/retrieving-symbol-information-by-address
// https://docs.microsoft.com/en-us/windows/win32/debug/using-symsrv#installation

#include <Windows.h>
#include <stdio.h>
#include <DbgHelp.h>

char* GetSym(HANDLE, PVOID);

int main()
{
    
    HANDLE hProcess= GetCurrentProcess();

    // Initialize symbol handler

    SymSetOptions(SYMOPT_UNDNAME | SYMOPT_DEFERRED_LOADS | SYMOPT_DEBUG);
    
    if (!SymInitialize(hProcess, NULL, TRUE))   //note: for symsrv.dll to be loaded successully it must be present in same directory than dbghelp.dll used
    {
        // SymInitialize failed        
        printf("\nSymInitialize returned error : %d", GetLastError());
        return FALSE;
    }

    printf("\nSymbol handler initialized");

    CHAR searchPath[MAX_PATH + 1];
    SymGetSearchPath(hProcess, searchPath, MAX_PATH + 1);
    printf("\nSymbol search path: %s", searchPath);

    CRITICAL_SECTION CriticalSection;

    // Initialize a dummy critical section (we don't even need to acquire it) to get access to the DebugInfo structure
    InitializeCriticalSectionEx(&CriticalSection, 4000, RTL_CRITICAL_SECTION_FLAG_FORCE_DEBUG_INFO);  // force to create the debug info structure            

    RTL_CRITICAL_SECTION cs = static_cast<RTL_CRITICAL_SECTION>(CriticalSection);

    // Walk the critical sections list via the DebugInfo 
    
    PLIST_ENTRY current = NULL;
    PLIST_ENTRY head = NULL;

    PRTL_CRITICAL_SECTION_DEBUG DebugInfo;

    printf("\nWalking critical sections list:");

    head = cs.DebugInfo->ProcessLocksList.Flink;
    current = head;

    int count = 0;

    while (head != current->Flink)
    {

        printf("\n-----------------------------------------");
        current = current->Flink;
        DebugInfo = CONTAINING_RECORD(current, RTL_CRITICAL_SECTION_DEBUG, ProcessLocksList);

        printf("\nCritsec %s at %p", GetSym(hProcess, (PVOID)DebugInfo->CriticalSection), DebugInfo->CriticalSection);
        printf("\nOwning thread %p", DebugInfo->CriticalSection->OwningThread);

        count++;
    }


    printf("\nFound %d critical sections", count);

    printf("\n\nPress Enter to exit");
    int c = getchar();    

    // Release resources used by the critical section object.
    DeleteCriticalSection(&CriticalSection);

    return 0;
}


// Attemp to get symbol of a critsec address
char* GetSym(HANDLE hProcess, PVOID address)
{

    DWORD64  dwDisplacement = 0;

    ULONG_PTR lpAddress = (ULONG_PTR)address;
    //DWORD64  dwAddress = (DWORD64) address;

    char buffer[sizeof(SYMBOL_INFO) + MAX_SYM_NAME * sizeof(TCHAR)];
    PSYMBOL_INFO pSymbol = (PSYMBOL_INFO)buffer;

    pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
    pSymbol->MaxNameLen = MAX_SYM_NAME;
    

    if (SymFromAddr(hProcess, (DWORD64)lpAddress, &dwDisplacement, pSymbol))    
    {
        // SymFromAddr returned success
        return (char*)&pSymbol->Name;
    }
    else
    {
        // SymFromAddr failed
        DWORD error = GetLastError();
        printf("\nSymFromAddr returned error : %d", error);

        return NULL;
    }

   
}