//#include "stdafx.h"
#include <iostream>
#include <Windows.h>
#include <string>
#include <iomanip>

int main()
{
    HANDLE hHeap = HeapCreate(0, 0x1000000, 0x10000000); // no options, initial 16M of committed on Heap, max 256MB, non growable heap
    HeapAlloc(hHeap, HEAP_GENERATE_EXCEPTIONS, 511000); // max. allocation size in bytes for non-growing heap
    HeapAlloc(hHeap, HEAP_GENERATE_EXCEPTIONS, 50000); // 50 000 more byest to be allocaed on same heap.

    std::cout << "Debug now, handle is 0x" << std::hex << std::setfill('0') << std::setw(sizeof(HANDLE)) << hHeap << std::endl;
    std::string dummy;
    std::getline(std::cin, dummy);
    return 0;
}