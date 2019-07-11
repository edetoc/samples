// MathFuncsConsumerB.cpp : Defines the entry point for the console application.
//

// this method uses implicit binding - see https://msdn.microsoft.com/en-us/library/9yd93633.aspx

#include <iostream>
#include <MathFuncsDll.h>

int main()
{
	std::cout << "Before calling Add (DLL should be already loaded in process memory...) Press Enter..." << std::endl;
	std::cin.get();

	std::cout << MathFuncs::MyMathFuncs::Add(32, 58) << "\n";

	std::cout << "After calling Add. Press Enter..." << std::endl;
	std::cin.get();
}
