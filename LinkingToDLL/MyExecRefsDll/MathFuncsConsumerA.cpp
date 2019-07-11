#include <iostream>
#include <windows.h>

// this method uses explicit binding - see https://msdn.microsoft.com/en-us/library/9yd93633.aspx

typedef double(*AddFunc)(double, double);
typedef void(*FunctionFunc)();

int main()
{
	AddFunc _AddFunc;
	
	HINSTANCE hInstLibrary = LoadLibrary(L"C:\\tmp\\hardlinkdll.dll");

	if (hInstLibrary)
	{

		std::cout << "DLL loaded with success" << std::endl;
	
		_AddFunc = (AddFunc)GetProcAddress(hInstLibrary, "?Add@MyMathFuncs@MathFuncs@@SANNN@Z");
		
		if (_AddFunc)
		{
			std::cout << "23 + 43 = " << _AddFunc(23, 43) << std::endl;
		}
		else
		{
			std::cout << "GetProcAddress failed ! with error: " << GetLastError() << std::endl;

		}

		std::cout << "Press enter to unload DLL" << std::endl;
		std::cin.get();
		FreeLibrary(hInstLibrary);
	}
	else
	{
		std::cout << "DLL Failed To Load! with error: " << GetLastError() << std::endl;
	}

	std::cout << "Press enter to exit program" << std::endl;
	std::cin.get();

	return 0;
}
