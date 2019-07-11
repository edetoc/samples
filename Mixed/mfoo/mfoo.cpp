#using <mscorlib.dll>
using namespace System;

public ref class ManagedFoo
{

public:
	ManagedFoo()
	{
		Console::WriteLine("Constructing ManagedFoo");
	
	}
	~ManagedFoo() { ShowDestruction(); }
	!ManagedFoo() { ShowDestruction(); }

	void ShowYourself()
	{
		array<Double>^ tab = gcnew array<Double>(1);
		tab[0] = Convert::ToDouble(System::DateTime::Now);
		System::IntPtr ptr = System::Runtime::InteropServices::Marshal::AllocHGlobal(System::Runtime::InteropServices::Marshal::SizeOf(tab[0]));

		Console::WriteLine("ManagedFoo");		
	}

private:
	void ShowDestruction()
	{
		
		Console::WriteLine("Destructing ManagedFoo");
	}
	
};
