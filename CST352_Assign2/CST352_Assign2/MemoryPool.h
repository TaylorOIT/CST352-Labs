// MemoryPool.h
// virtual base class for all memory pools

#pragma once
class MemoryPool
{
	protected:
		MemoryPool(unsigned int poolSize);

	public:
		virtual ~MemoryPool();
		virtual void* Allocate(unsigned int nBytes);
		virtual void Free(void* block);
		virtual void DebugPrint();
};

