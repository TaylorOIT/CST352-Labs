// FirstFitPool.h
// memory pool using first fit allocaiton strategy

#pragma once
#include "MemoryPool.h"

class FirstFitPool : public MemoryPool
{
	protected:
		virtual std::vector<Chunk>::iterator FindAvailableChunk(unsigned int nBytes);
	public:
		FirstFitPool(unsigned int poolSize) : MemoryPool(poolSize) {}
};

