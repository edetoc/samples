//#include "stdafx.h"
#include <iostream>
#include <Windows.h>
#include <string>
#include <iomanip>

int main()
{
    //HANDLE hHeap = HeapCreate(0, 0x1000000, 0x10000000); // no options, initial 16M of committed on Heap, max 256MB, non growable heap

    HANDLE hHeap = HeapCreate(0, 1000000, 0x10000000);// no options, initial 1M of committed on Heap, max 256MB, non growable heap
    std::cout << "Debug now, handle is 0x" << std::hex << std::setfill('0') << std::setw(sizeof(HANDLE)) << hHeap << std::endl;

    for (int i = 0; i < 10; i++)
    {
        std::cout << "iter: " << i << std::endl;

        LPVOID lpRes;
        lpRes = HeapAlloc(hHeap, HEAP_GENERATE_EXCEPTIONS | HEAP_ZERO_MEMORY, 500000);  // 500000 bytes

       // HeapFree(hHeap,0 , lpRes);

        std::cout << "Press a key" << std::endl;
        std::string dummy;
        std::getline(std::cin, dummy);

    }

    //HeapAlloc(hHeap, HEAP_GENERATE_EXCEPTIONS, 511000); // max. allocation size in bytes for non-growing heap
    //HeapAlloc(hHeap, HEAP_GENERATE_EXCEPTIONS, 50000); // 50 000 more byest to be allocaed on same heap.

    // std::cout << "Debug now, handle is 0x" << std::hex << std::setfill('0') << std::setw(sizeof(HANDLE)) << hHeap << std::endl;
  /*  std::string dummy;
    std::getline(std::cin, dummy);*/
    return 0;
}