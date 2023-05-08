#include "StringQueue.h"

StringQueue::StringQueue(MemoryPool* pool) :pool(pool)
{
}

void StringQueue::Insert(const char* s)
{
	char* newString = (char*)pool->Allocate(strlen(s) + 1);
	strcpy_s(newString, strlen(s) + 1, s);
	theQueue.push(newString);
	pool->DebugPrint();
}

const char* StringQueue::Peek() const
{
	return "Hello";
}

void StringQueue::Remove()
{
}
