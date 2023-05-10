#include "StringQueue.h"

StringQueue::StringQueue(MemoryPool* pool) :pool(pool)
{
}

void StringQueue::Insert(const char* s)
{
	// allocate memory in the pool to hold the string and its null terminator
	try{
		char* newString = (char*)pool->Allocate(strlen(s) + 1);
		// copy the string contents
		strcpy_s(newString, strlen(s) + 1, s);

		// push onto the queue
		theQueue.push(newString);
	}
	catch(OutofMemoryException){
		throw FullException();
	}


	pool->DebugPrint();
}

const char* StringQueue::Peek() const
{
	if (theQueue.empty())
		throw EmptyException();

	return theQueue.front();
}

void StringQueue::Remove()
{
	if (theQueue.empty())
		throw EmptyException();

	// grab the string at the front of the queue
	char* s = theQueue.front();

	// free up the memory in the pool
	pool->Free(s);

	// pop the string off the queue
	theQueue.pop();

	pool->DebugPrint();
}
