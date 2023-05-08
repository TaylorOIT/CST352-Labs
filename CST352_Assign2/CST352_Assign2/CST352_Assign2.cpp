// Taylor Boyd

#include <iostream>
#include "MemoryPool.h"
#include "StringQueue.h"
#include "FirstFitPool.h"

int main()
{
    FirstFitPool pool(100);
    StringQueue queue(&pool);

    queue.Insert("foo");
    queue.Insert("bar");
    std::cout << queue.Peek() << std::endl;
    queue.Remove();
    std::cout << queue.Peek() << std::endl;
    queue.Remove();
    
}