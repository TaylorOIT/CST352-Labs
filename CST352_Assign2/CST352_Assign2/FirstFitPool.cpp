#include "FirstFitPool.h"

std::vector<MemoryPool::Chunk>::iterator FirstFitPool::FindAvailableChunk(unsigned int nBytes)
{
    // First Fit Algorithm
    // find the first available chunk that is big enough to fit nBytes (at least that big)

    for (std::vector<MemoryPool::Chunk>::iterator iter = chunks.begin(); iter != chunks.end(); iter++) {
        if (!iter->allocated && iter->size >= nBytes) {
            return iter;
        }
    }

    // couldn't find one
    return chunks.end();
}
