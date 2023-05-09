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
	return new unsigned char[nBytes];
}

void MemoryPool::Free(void* block)
{
}

void MemoryPool::DebugPrint() const
{
}
