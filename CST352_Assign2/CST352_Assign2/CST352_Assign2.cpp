// Taylor Boyd

#include <iostream>
#include "MemoryPool.h"
#include "StringQueue.h"
#include "FirstFitPool.h"

int main()
{
    try {
        FirstFitPool pool(100);
        StringQueue queue(&pool);

        queue.Insert("foo");
        queue.Insert("bar");
        std::cout << queue.Peek() << std::endl;
        queue.Remove();
        std::cout << queue.Peek() << std::endl;
        queue.Remove();
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