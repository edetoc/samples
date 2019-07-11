// DLLSample.cpp : Defines the exported functions for the DLL application.
//


#include "stdafx.h"
#include "DLLSample.h"

static int n = 0;

int func()
{
	return n++;

}
