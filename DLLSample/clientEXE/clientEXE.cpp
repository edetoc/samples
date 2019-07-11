// clientEXE.cpp : Defines the entry point for the console application.
//

#include <iostream>
#include <Windows.h>

typedef int(*function)();


int main()
{

	function _func;


	for (int i = 0; i < 100; i++)
	{
		HINSTANCE hInstLibrary = LoadLibrary(L"DLLSample.dll");

		if (hInstLibrary)
		{
			std::cout << "LoadLibrary succeeded" << std::endl;

			_func = (function)GetProcAddress(hInstLibrary, "func");

			if (_func)
			{

				for (int j = 0; j < 5; j++)
				{

					std::cout << _func() << std::endl;
				}

			}
			else
			{
				std::cout << "GetProcAddress failed with error: " << GetLastError() << std::endl;

			}
			if (FreeLibrary(hInstLibrary))
			{
				std::cout << "FreeLibrary succeeded" << std::endl;

			}
			else
			{
				std::cout << "FreeLibrary failed with error: " << GetLastError() << std::endl;

			}
		}
		else
		{
			std::cout << "DLL Failed To Load! with error: " << GetLastError() << std::endl;

		}

	}

	return 0;
}
