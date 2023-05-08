// MemoryPool.h
// virtual base class for all memory pools

#pragma once
#include <vector>

class MemoryPool
{
	protected:
		class Chunk {
			public :
				unsigned int startingIndex;
				unsigned int size;
				bool allocated;
				Chunk(unsigned int startingIndex, unsigned int size, bool allocated) :
					startingIndex(startingIndex), size(size), allocated(allocated) {}
		};
		unsigned char* pool;
		std::vector<Chunk> chunks;

		MemoryPool(unsigned int poolSize);
		virtual std::vector<Chunk>::iterator FindAvailableChunk(unsigned int nBytes) = 0;

	public:
		virtual ~MemoryPool();
		virtual void* Allocate(unsigned int nBytes);
		virtual void Free(void* block);
		virtual void DebugPrint() const;
};

