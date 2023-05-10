// StringQueue.h
// queue of strings, where they are store in a memory pool

#pragma once
#include "MemoryPool.h"
#include <queue>

class FullException 
{

};

class EmptyException 
{

};

class StringQueue
{
	private:
		MemoryPool* pool;
		std::queue<char*> theQueue;
	public:
		StringQueue(MemoryPool* pool);
		void Insert(const char* s);
		const char* Peek() const;
		void Remove();
};

