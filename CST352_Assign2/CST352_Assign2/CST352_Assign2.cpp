// Taylor Boyd

#include <iostream>
#include "MemoryPool.h"
#include "StringQueue.h"
#include "FirstFitPool.h"
#include "BestFitPool.h"

void TestFindAlgorithm(MemoryPool* pool)
{
    std::cout << "inital pool..." << std::endl;
    pool->DebugPrint();
    StringQueue queue(pool);

    queue.Insert("01234567890123456789012345678901234567890123456789");
    queue.Insert("0123456789");
    queue.Remove();
    queue.Insert("012345678901234567890123456789");

    std::cout << "final pool..." << std::endl;
    pool->DebugPrint();
}

int main()
{
    try {
        FirstFitPool firstFitPool(100);
        BestFitPool bestFitPool(100);
        
        TestFindAlgorithm(&firstFitPool);
        TestFindAlgorithm(&bestFitPool);
        //queue.Insert("foo");
        //queue.Insert("bar");
        //std::cout << queue.Peek() << std::endl;
        //queue.Remove();
        //std::cout << queue.Peek() << std::endl;
        //queue.Remove();
        //pool.DebugPrint();
    }
    catch (FullException) {
        std::cout << "FullException!" << std::endl;
    }
    catch (EmptyException) {
        std::cout << "EmptyException!" << std::endl;
    }
    catch (OutofMemoryException) {
        std::cout << "OutofMemoryException!" << std::endl;
    }
}