
#include <iostream>
#include<Windows.h>

void leak() {
    
    // allocate 1Mb from heap
    char* name = new char[1000*1024];

    //delete name;
}


int main()
{
    std::cout << "Running...\n";

    int i = 0;

    while (i < 1000 )
    {
        std::cout << "i: " << i << "\n";
        leak();
        Sleep(20);
        i++;
    }

    std::cout << "end...\n";

    std::cout << "press a key to terminate...\n";

    getchar();

}
