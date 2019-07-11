// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include<stdio.h>
// Logger
// See http://www.codeproject.com/Articles/584794/Simple-logger-for-Cplusplus
#include <fstream>
#include "Logger.h"
#include "threading.h"
#include<Uxtheme.h>

using namespace framework::Diagnostics;
using namespace framework::Threading;

void DoNCPaint(HWND,WPARAM);

WNDPROC OldWndProc;

CLogger<CNoLock> logger(LogLevel::Info, _T("MyHookApp"));;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		logger.AddOutputStream(new std::wofstream("c:\\tmp\\hook.log"), true, framework::Diagnostics::LogLevel::Info);
		WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("logger initialized"));		
		break;
	case DLL_THREAD_ATTACH:		
		break;
	case DLL_THREAD_DETACH:		
		break;
	case DLL_PROCESS_DETACH:		
		WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("logger uninitialized"));
		break;
	}

	return TRUE;
}

// subclassed window proc
LRESULT CALLBACK NewWndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	
	WRITELOG(logger, framework::Diagnostics::LogLevel::Info, L"Message: 0x%x", uMsg);

	switch (uMsg)
		{
		case WM_NCACTIVATE:			// 0x0086
			WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("WM_NCACTIVATE received"));
			//DoNCPaint(hWnd, wParam);
			return 0;

		case WM_NCPAINT:			// 0x0085
			WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("WM_NCPAINT received"));
			DoNCPaint(hWnd, wParam);
			return 0;

		case WM_DESTROY:
			WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("WM_DESTROY received"));
			// remove the wndProc subclass
			SetWindowLongPtr(hWnd, GWLP_WNDPROC, (LONG_PTR)OldWndProc);
			break;
		default: 
			//WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("Switch-Case default"));
			return CallWindowProc(OldWndProc, hWnd, uMsg, wParam, lParam);

		}
	return CallWindowProc(OldWndProc, hWnd, uMsg, wParam, lParam);

}


void DoNCPaint(HWND hWnd, WPARAM wParam)
{
	
	HDC hdc;

	//hdc = GetDCEx(hWnd, (HRGN)wParam, DCX_WINDOW | DCX_CACHE | DCX_INTERSECTRGN | DCX_LOCKWINDOWUPDATE);
	
	hdc = GetWindowDC(hWnd);
	WRITELOG(logger, framework::Diagnostics::LogLevel::Info, L"hdc: 0x%x", hdc);

	RECT rect;
	GetWindowRect(hWnd, &rect);

	HBRUSH b;
	b = CreateSolidBrush(RGB(0, 180, 180));   // RGB converter here http://www.psyclops.com/tools/rgb/
	SelectObject(hdc, b);

	HPEN pe;
	pe = CreatePen(PS_SOLID, 1, RGB(90, 90, 90));
	SelectObject(hdc, pe);

	//The Rectangle function draws a rectangle.
	//The rectangle is outlined by using the current pen and filled by using the current brush.
	Rectangle(hdc, 0, 0, (rect.right - rect.left), (rect.bottom - rect.top));

	// Display some text in the title
	TCHAR text[] = L"this is some text";
	SetBkColor(hdc, RGB(0, 180, 180));
	TextOut(hdc, 5, 5, text, ARRAYSIZE(text));

	DeleteObject(pe);
	DeleteObject(b);

	// Area for min, max, close buttons
	RECT closeRect;
	closeRect.left = rect.right - rect.left - 20;
	closeRect.top = GetSystemMetrics(SM_CYFRAME);
	closeRect.right = rect.right - rect.left - 5;
	closeRect.bottom = GetSystemMetrics(SM_CYSIZE);

	RECT maxRect;
	maxRect.left = rect.right - rect.left - 40;
	maxRect.top = GetSystemMetrics(SM_CYFRAME);
	maxRect.right = rect.right - rect.left - 25;
	maxRect.bottom = GetSystemMetrics(SM_CYSIZE);

	RECT minRect;
	minRect.left = rect.right - rect.left - 60;
	minRect.top = GetSystemMetrics(SM_CYFRAME);
	minRect.right = rect.right - rect.left - 45;
	minRect.bottom = GetSystemMetrics(SM_CYSIZE);


	// Put the close button on the caption	
	DrawFrameControl(hdc, &minRect, DFC_CAPTION,  DFCS_CAPTIONMIN);
	DrawFrameControl(hdc, &maxRect, DFC_CAPTION, DFCS_CAPTIONMAX);
	DrawFrameControl(hdc, &closeRect, DFC_CAPTION, DFCS_CAPTIONCLOSE);

	ReleaseDC(hWnd, hdc);

}

// this is our hook. It monitors messages before the system sends them to the destination window procedure
LRESULT  inject(int nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode < 0)  // do not process message 
		return CallNextHookEx(NULL, nCode, wParam, lParam);

	if (nCode == HC_ACTION) 
	{
	
		MSG* msg = (MSG*)(lParam);
		
		WRITELOG(logger, framework::Diagnostics::LogLevel::Info, L"Message: 0x%x", msg->message );

		if (OldWndProc == NULL)
		{
			// messages in which we are interested are going to be handled in the subclassed window proc
			WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("subclassing window proc"));			
			OldWndProc = (WNDPROC)SetWindowLongPtr(msg->hwnd, GWLP_WNDPROC, (LONG_PTR)NewWndProc);
			WRITELOG(logger, framework::Diagnostics::LogLevel::Info, _T("subclassing window proc done"));
		}
		else
		{
			// WndProc subclassing is already done
			
			// subclassed WndProc will be removed on WM_DESTROY

		}

	}

	return(CallNextHookEx(NULL, nCode, wParam, lParam));
}
