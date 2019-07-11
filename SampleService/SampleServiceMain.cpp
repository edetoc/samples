/*

This sample code is from:
http://www.codeproject.com/Articles/499465/Simple-Windows-Service-in-Cplusplus

Service installation
C:\>sc create "My Sample Service" binPath= C:\SampleService.exe

Service removal
C:\sc delete "My Sample Service"

*/ 
	

#include <Windows.h>
#include <tchar.h>
#include"Log.h"

//SERVICE_STATUS        g_ServiceStatus = {0};
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
HANDLE                g_ServiceStopEvent = INVALID_HANDLE_VALUE;

Log *pLog = new Log("c:\\tmp\\SampleServiceLog.log");

VOID WINAPI ServiceMain (DWORD argc, LPTSTR *argv);
DWORD WINAPI ServiceCtrlHandler(DWORD, DWORD, LPVOID, LPVOID);
DWORD WINAPI ServiceWorkerThread (LPVOID lpParam);
BOOL ReportServiceStatus(DWORD dwStatus, DWORD dwErrorCode, DWORD dwWaitHint);
	

#define SERVICE_NAME  _T("My Sample Service")

int _tmain (int argc, TCHAR *argv[])
{

	pLog->Write("My Sample Service: Main: Entry");

    SERVICE_TABLE_ENTRY ServiceTable[] = 
    {
        {SERVICE_NAME, (LPSERVICE_MAIN_FUNCTION) ServiceMain},
        {NULL, NULL}
    };

    if (StartServiceCtrlDispatcher (ServiceTable) == FALSE)
    {
		pLog->Write("My Sample Service: Main: StartServiceCtrlDispatcher returned error");
       return GetLastError ();
    }

	pLog->Write("My Sample Service: Main: Exit");
    return 0;
}


VOID WINAPI ServiceMain (DWORD argc, LPTSTR *argv)
{
    DWORD Status = E_FAIL;

	pLog->Write("My Sample Service: ServiceMain: Entry");

	g_StatusHandle = RegisterServiceCtrlHandlerEx(SERVICE_NAME, (LPHANDLER_FUNCTION_EX)ServiceCtrlHandler,0);

    if (g_StatusHandle == NULL) 
    {
		pLog->Write("My Sample Service: ServiceMain: RegisterServiceCtrlHandler returned error");
        goto EXIT;
    }

    // Tell the service controller we are starting
	ReportServiceStatus(SERVICE_START_PENDING, NO_ERROR, 5000);

   /* ZeroMemory (&g_ServiceStatus, sizeof (g_ServiceStatus));
    g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
    g_ServiceStatus.dwControlsAccepted = 0;
    g_ServiceStatus.dwCurrentState = SERVICE_START_PENDING;
    g_ServiceStatus.dwWin32ExitCode = 0;
    g_ServiceStatus.dwServiceSpecificExitCode = 0;
    g_ServiceStatus.dwCheckPoint = 0;

    if (SetServiceStatus (g_StatusHandle, &g_ServiceStatus) == FALSE) 
    {
		pLog->Write("My Sample Service: ServiceMain: SetServiceStatus returned error");
    }*/

    /* 
     * Perform tasks neccesary to start the service here
     */
	pLog->Write("My Sample Service: ServiceMain: Performing Service Start Operations");

    // Create stop event to wait on later.
    g_ServiceStopEvent = CreateEvent (NULL, TRUE, FALSE, NULL);
    if (g_ServiceStopEvent == NULL) 
    {
		pLog->Write("My Sample Service: ServiceMain: CreateEvent(g_ServiceStopEvent) returned error");

		ReportServiceStatus(SERVICE_STOPPED, GetLastError(), 0);

      /*  g_ServiceStatus.dwControlsAccepted = 0;
        g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
        g_ServiceStatus.dwWin32ExitCode = GetLastError();
        g_ServiceStatus.dwCheckPoint = 1;

        if (SetServiceStatus (g_StatusHandle, &g_ServiceStatus) == FALSE)
	    {
			pLog->Write("My Sample Service: ServiceMain: SetServiceStatus returned error");
	    }*/
        goto EXIT; 
    }    

	// Tell the service controller we are started
	ReportServiceStatus(SERVICE_RUNNING, NO_ERROR, 0);


 //   
	////g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_PRESHUTDOWN;
	//g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN;
 //   g_ServiceStatus.dwCurrentState = SERVICE_RUNNING;
 //   g_ServiceStatus.dwWin32ExitCode = 0;
 //   g_ServiceStatus.dwCheckPoint = 0;

 //   if (SetServiceStatus (g_StatusHandle, &g_ServiceStatus) == FALSE)
 //   {
	//	pLog->Write("My Sample Service: ServiceMain: SetServiceStatus returned error");
 //   }

    // Start the thread that will perform the main task of the service
    HANDLE hThread = CreateThread (NULL, 0, ServiceWorkerThread, NULL, 0, NULL);

	pLog->Write("My Sample Service: ServiceMain: Waiting for Worker Thread to complete");

    // Wait until our worker thread exits effectively signaling that the service needs to stop
    WaitForSingleObject (hThread, INFINITE);
    
	pLog->Write("My Sample Service: ServiceMain: Worker Thread Stop Event signaled");
    
    
    /* 
     * Perform any cleanup tasks
     */
	pLog->Write("My Sample Service: ServiceMain: Performing Cleanup Operations");

    CloseHandle (g_ServiceStopEvent);

	ReportServiceStatus(SERVICE_STOPPED, NO_ERROR, 0);

   /* g_ServiceStatus.dwControlsAccepted = 0;
    g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
    g_ServiceStatus.dwWin32ExitCode = 0;
    g_ServiceStatus.dwCheckPoint = 3;

    if (SetServiceStatus (g_StatusHandle, &g_ServiceStatus) == FALSE)
    {
		pLog->Write("My Sample Service: ServiceMain: SetServiceStatus returned error");
    }*/
    
    EXIT:
	pLog->Write("My Sample Service: ServiceMain: Exit");

    return;
}


//VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode)
DWORD WINAPI ServiceCtrlHandler(DWORD CtrlCode, DWORD  dwEventType, LPVOID lpEventData, LPVOID lpContext)
{
	DWORD ret = NO_ERROR;

	pLog->Write("My Sample Service: ServiceCtrlHandler: Entry");

    switch (CtrlCode) 
	{

     case SERVICE_CONTROL_STOP :

		 pLog->Write("My Sample Service: ServiceCtrlHandler: SERVICE_CONTROL_STOP Request");

  //      if (g_ServiceStatus.dwCurrentState != SERVICE_RUNNING)
  //         break;

  //      /* 
  //       * Perform tasks neccesary to stop the service here 
  //       */
  //      
  //      g_ServiceStatus.dwControlsAccepted = 0;
  //      g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
  //      g_ServiceStatus.dwWin32ExitCode = 0;
  //      g_ServiceStatus.dwCheckPoint = 4;

  //      if (SetServiceStatus (g_StatusHandle, &g_ServiceStatus) == FALSE)
		//{
		//	pLog->Write("My Sample Service: ServiceCtrlHandler: SetServiceStatus returned error");
		//}

		ReportServiceStatus(SERVICE_STOP_PENDING, NO_ERROR, 5000);

        // This will signal the worker thread to start shutting down
        SetEvent (g_ServiceStopEvent);

        break;


	 case SERVICE_CONTROL_PRESHUTDOWN:
		 
		 /*

		 Note : according to below link, the default preshutdown time-out value is 180,000 milliseconds (three minutes).

		 SERVICE_PRESHUTDOWN_INFO structure
		 https://msdn.microsoft.com/en-us/library/windows/desktop/ms685961(v=vs.85).aspx

		 */
		 
		 pLog->Write("My Sample Service: ServiceCtrlHandler: SERVICE_CONTROL_PRESHUTDOWN Request");

		 ReportServiceStatus(SERVICE_STOP_PENDING, NO_ERROR, 0);

		/* g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
	
		 if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		 {
			pLog->Write("My Sample Service: ServiceCtrlHandler: SetServiceStatus returned error");
		 }*/

		 pLog->Write("My Sample Service: start of cleanup work");

		 for (int i = 0; i < 30; i++)
		 {
			  pLog->Write("My Sample Service: doing some cleanup work");
			  Sleep(1000);	
		 }
		 
		 pLog->Write("My Sample Service: end of cleanup work end");

		 break;

	 case SERVICE_CONTROL_SHUTDOWN:

		 pLog->Write("My Sample Service: ServiceCtrlHandler: SERVICE_CONTROL_SHUTDOWN Request");

		 ReportServiceStatus(SERVICE_STOP_PENDING, NO_ERROR, 25000);

		 ////g_ServiceStatus.dwControlsAccepted = 0;
		 //g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
		 //g_ServiceStatus.dwWin32ExitCode = NO_ERROR;
		 //g_ServiceStatus.dwCheckPoint = 5;		 
		 //g_ServiceStatus.dwWaitHint = 25000;

		 //if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
		 //{
			// pLog->Write("My Sample Service: ServiceCtrlHandler: SetServiceStatus returned error");
		 //}

		 pLog->Write("My Sample Service: cleanup work start");

		 for (int i = 0; i < 20; i++)
		 {
			 pLog->Write("My Sample Service: doing some cleanup work");
			 Sleep(1000);

		/*	 g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
			 g_ServiceStatus.dwCheckPoint++;
			 g_ServiceStatus.dwWaitHint = 25000;

			 if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
			 {
				 pLog->Write("My Sample Service: ServiceCtrlHandler: SetServiceStatus returned error");
			 }*/
			
		 }
		 
		 pLog->Write("My Sample Service: cleanup work end");

		 // This will signal the worker thread to start shutting down
		 SetEvent(g_ServiceStopEvent);

		 break;

     default:
         break;
    }

	pLog->Write("My Sample Service: ServiceCtrlHandler: Exit");

	return ret;
}



BOOL
ReportServiceStatus(
IN DWORD dwStatus,
IN DWORD dwErrorCode,
IN DWORD dwWaitHint
)
{
	BOOL bResult = FALSE;

	static DWORD dwCheckPoint = 1;
	SERVICE_STATUS ServiceStatus = { 0 };

	if (g_StatusHandle != NULL)
	{
		ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;

		ServiceStatus.dwCurrentState = dwStatus;

		if (dwStatus == SERVICE_START_PENDING)
		{
			ServiceStatus.dwControlsAccepted = 0;
		}
		else
		{
			ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN;
		}

		ServiceStatus.dwWin32ExitCode = dwErrorCode;

		ServiceStatus.dwServiceSpecificExitCode = 0;

		if (dwStatus == SERVICE_RUNNING ||
			dwStatus == SERVICE_STOPPED)
		{
			ServiceStatus.dwCheckPoint = 0;
		}
		else
		{
			ServiceStatus.dwCheckPoint = dwCheckPoint++;
		}

		ServiceStatus.dwWaitHint = dwWaitHint;

		if (SetServiceStatus(g_StatusHandle, &ServiceStatus) == FALSE)
		{
			//WriteLog(L"ReportServiceStatus: SetServiceStatus() failed! GLE = 0x%x", GetLastError());
			pLog->Write("ReportServiceStatus: SetServiceStatus() failed!");

			return FALSE;
		}

		bResult = TRUE;
	}
	else
	{
		
		pLog->Write("ReportServiceStatus: g_StatusHandle is NULL!");
	}

	return bResult;
}


DWORD WINAPI ServiceWorkerThread(LPVOID lpParam)
{
	pLog->Write("My Sample Service: ServiceWorkerThread: Entry");

	//  Periodically check if the service has been requested to stop
	while (WaitForSingleObject(g_ServiceStopEvent, 0) != WAIT_OBJECT_0)
	{
		/*
		* Perform main service function here
		*/

		//  Simulate some work by sleeping
		Sleep(3000);
	}

	pLog->Write("My Sample Service: ServiceWorkerThread: Exit");

	return ERROR_SUCCESS;
}


