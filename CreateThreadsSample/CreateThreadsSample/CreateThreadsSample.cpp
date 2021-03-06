// CreateThreadsSample.cpp : This file contains the 'main' function. Program execution begins and ends there.
//


#include <windows.h>
#include <tchar.h>
#include <strsafe.h>
#include <processthreadsapi.h>

#define MAX_THREADS 10

DWORD WINAPI MyThreadFunction(LPVOID lpParam);
void ErrorHandler(LPTSTR lpszFunction);


int _tmain()
{
	
	DWORD   dwThreadIdArray[MAX_THREADS];
	HANDLE  hThreadArray[MAX_THREADS];

	// Create MAX_THREADS worker threads.

	for (int i = 0; i < MAX_THREADS; i++)
	{		
		// Create the thread to begin execution on its own.

		hThreadArray[i] = CreateThread(
			NULL,                   // default security attributes
			0,                      // use default stack size  
			MyThreadFunction,       // thread function name
			NULL,					// argument to thread function 
			0,                      // use default creation flags 
			&dwThreadIdArray[i]);   // returns the thread identifier 


		// Check the return value for success.
		// If CreateThread fails, terminate execution. 
		// This will automatically clean up threads and memory. 

		if (hThreadArray[i] == NULL)
		{
			TCHAR func[] = _T("CreateThread");
			ErrorHandler(func);
			ExitProcess(3);
		}

		// wait 1s before creating new thread
		Sleep(1000L);

	} 

	// Wait until all threads have terminated.

	WaitForMultipleObjects(MAX_THREADS, hThreadArray, TRUE, INFINITE);

	// Close all thread handles and free memory allocations.

	for (int i = 0; i < MAX_THREADS; i++)
	{
		CloseHandle(hThreadArray[i]);
		
	}

	return 0;
}


DWORD WINAPI MyThreadFunction(LPVOID lpParam)
{
	
	ULONG_PTR LowLimit, HighLimit;
	GetCurrentThreadStackLimits(&LowLimit, &HighLimit);

#ifdef _WIN64
	
	printf("Stack=%llX\n", LowLimit);
#elif _WIN32	
	/*unsigned long LowLimit, HighLimit;
	GetCurrentThreadStackLimits(&LowLimit, &HighLimit);*/
	printf("Stack=%08X\n", LowLimit);
#endif
	
	return 0;
}



void ErrorHandler(LPTSTR lpszFunction)
{
	// Retrieve the system error message for the last-error code.

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

	// Display the error message.

	lpDisplayBuf = (LPVOID)LocalAlloc(LMEM_ZEROINIT,
		(lstrlen((LPCTSTR)lpMsgBuf) + lstrlen((LPCTSTR)lpszFunction) + 40) * sizeof(TCHAR));
	StringCchPrintf((LPTSTR)lpDisplayBuf,
		LocalSize(lpDisplayBuf) / sizeof(TCHAR),
		TEXT("%s failed with error %d: %s"),
		lpszFunction, dw, lpMsgBuf);
	MessageBox(NULL, (LPCTSTR)lpDisplayBuf, TEXT("Error"), MB_OK);

	// Free error-handling buffer allocations.

	LocalFree(lpMsgBuf);
	LocalFree(lpDisplayBuf);
}