#include "MemoryPool.h"

MemoryPool::MemoryPool(unsigned int poolSize)
{
	// allocate memory for the pool
	pool = new unsigned char[poolSize];

	// the initial pool has 1 chunk that is unallocated and overs the entire pool's memory
	chunks.push_back(Chunk(0, poolSize, false));
}

MemoryPool::~MemoryPool()
{
}

void* MemoryPool::Allocate(unsigned int nBytes)
{
	// Find or create an available chuck that is at least nBytes large
	// if we can't, then we throw an OutOfMemory exception
	// defer to the derived classes, where the allocation algorithm is implemented.
	// when done, the chunks vector should refelect the new structure of the pool
	//			  we can return a pointer to the newly allocated chunk in the pool
	
	unsigned char* block = nullptr; // pointer to the newly allocated chunk

	// defer to the derived classes, where the allocation algorithm is implemented
	// to find the available chunk
	std::vector<Chunk>::iterator iter = FindAvailableChunk(nBytes);
	if (iter == chunks.end()) {
		// can't find enough memory.
		throw OutofMemoryException();
	}

	// we found an available chunk, calculate the block pointer for that chunk
	block = pool + iter->startingIndex;

	// return pointer to application
	return block;
}

void MemoryPool::Free(void* block)
{
}

void MemoryPool::DebugPrint() const
{
}
