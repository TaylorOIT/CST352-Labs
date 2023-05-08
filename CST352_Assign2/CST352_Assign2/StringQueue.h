// StringQueue.h
// queue of strings, where they are store in a memory pool

#pragma once
#include "MemoryPool.h"

class StringQueue
{
	public:
		StringQueue(MemoryPool* pool);
		void Insert(char* s);
		char Peek();
		void Remove();
};

