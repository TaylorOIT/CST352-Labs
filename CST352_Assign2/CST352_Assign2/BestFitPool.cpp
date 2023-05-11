#include "BestFitPool.h"


std::vector<MemoryPool::Chunk>::iterator BestFitPool::FindAvailableChunk(unsigned int nBytes)
{
    // Best Fit Algorithm
    // find the smallest available chunk that is big enough to fit nBytes (at least that big)
    // find the first chunk that is big enough and then
    // continue through the rest of the chunks, keep track of the smallest one we find that is big enough

    // find first fit
    std::vector<MemoryPool::Chunk>::iterator bestIter;
    for (bestIter = chunks.begin(); bestIter != chunks.end(); bestIter++) {
        if (!bestIter->allocated && bestIter->size >= nBytes) {
            // we found the first fit....
            break;
        }
    }

    // find the best fit from there
    for (std::vector<MemoryPool::Chunk>::iterator chunkIter = bestIter; chunkIter != chunks.end(); chunkIter++) {
        // if we find a better one, remember that
        if (!chunkIter->allocated && chunkIter->size >= nBytes && chunkIter->size < bestIter->size) {
            bestIter = chunkIter;
        }
    }

    // couldn't find one
    return bestIter;
}

