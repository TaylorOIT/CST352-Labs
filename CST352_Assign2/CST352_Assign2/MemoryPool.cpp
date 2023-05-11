#include "MemoryPool.h"
#include <iostream>

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
	// defer to the derived classes, where the allocation algorithm is implemented
	// when done, the chunks vector should refelect the new structure of the pool
	//			  we can return a pointer to the newly allocated chunk in the pool
	
	unsigned char* block = nullptr; // pointer to the newly allocated chunk

	// defer to the derived classes, where the allocation algorithm is implemented
	// to find an available chunk that we can allocate
	std::vector<Chunk>::iterator iter = FindAvailableChunk(nBytes);
	if (iter == chunks.end()) {
		// can't find enough memory.
		throw OutofMemoryException();
	}

	// split the chunk if necessary
	if (iter->size > nBytes) {
		// split into two chunks, one that is exactly nBytes and the other is iter->size-nBytes
		
		// insert a new chunk that represents exactly he bytes requested, which is allocated
		iter = chunks.insert(iter, Chunk(iter->startingIndex, nBytes, true));

		// fix up the original chunk to represent the extra available memory
		std::next(iter)->startingIndex += nBytes;
		std::next(iter)->size -= nBytes;
	}

	// allocate the chunk
	iter->allocated = true;

	// we found an available chunk, calculate the block pointer for that chunk
	block = pool + iter->startingIndex;

	// return pointer to application
	return block;
}

void MemoryPool::Free(void* block)
{
	// find the chunk corresponding to the block to be freed
	std::vector<Chunk>::iterator iter;
	for (iter = chunks.begin(); iter != chunks.end() && (pool + iter->startingIndex) != block; iter++);
	
	// no such chunk corresponding with that block!
	if (iter == chunks.end())
		return;
	
	// mark the chunk as available
	iter->allocated = false;

	// coalesce, if possible...

	// w/ previous chunk if it exists and is also available
	if (iter != chunks.begin() && !std::prev(iter)->allocated)
	{
		// coalesce!
		iter->startingIndex = std::prev(iter)->startingIndex;
		iter->size += std::prev(iter)->size;
		iter = chunks.erase(std::prev(iter));
	}

	// same thing with the next chunk
	if (std::next(iter) != chunks.end() && !std::next(iter)->allocated)
	{
		// coalesce!
		iter->size += std::next(iter)->size;
		chunks.erase(std::next(iter));
	}
}

void MemoryPool::DebugPrint() const
{

	std::cout << std::endl;
	std::cout << "MemoryPool {" << std::endl;
	for (auto c : chunks) {
		std::cout << "\tindex: " << c.startingIndex << ", size: " << c.size << ", allocated: " << c.allocated
			<< std::endl;
	}
	std::cout << "}" << std::endl;
	std::cout << std::endl;
}
