// BestFitPool.h
// memory pool using best fit allocation strategy
#pragma once

#pragma once
#include "MemoryPool.h"

class BestFitPool : public MemoryPool
{
protected:
	virtual std::vector<Chunk>::iterator FindAvailableChunk(unsigned int nBytes);
public:
	BestFitPool(unsigned int poolSize) : MemoryPool(poolSize) {}
};

