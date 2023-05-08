// MemoryPool.h
// virtual base class for all memory pools

#pragma once
class MemoryPool
{
	public:
		MemoryPool(unsigned int poolSize);
		virtual void* Allocate(unsigned int nBytes);
		virtual void Free(void* block);
		virtual void DebugPrint();

};

