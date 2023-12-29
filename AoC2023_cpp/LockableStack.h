#pragma once

#include <stack>
#include <chrono>
#include <mutex>
#include <condition_variable>

namespace ThreadsNThings
{
    //data passed between worker threads
    template<class T>
    class LockableStack
    {
    public:
        using value_type = T;

        void push(T&& val)
        {
            std::lock_guard<std::mutex> lock(mutex);
            stack.push(val);
            populatedNotifier.notify_one();
        }

        void push_back(T&& val)
        {
            push(std::move(val));
        }

        void push_queue(std::queue<T>* inqueue)
        {
            std::lock_guard<std::mutex> lock(mutex);

            while (inqueue->size() > 0)
            {
                stack.push(std::move(inqueue->front()));
                inqueue->pop();
            }

            populatedNotifier.notify_all();
        }

        bool try_pop(T* item, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !stack.empty(); }))
                return false;

            *item = std::move(stack.front());
            stack.pop();

            return true;
        }

        int try_pop_range(std::queue<T>* nextQueue, int maxToTake, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !stack.empty(); }))
                return 0;

            int count = 0;
            while (maxToTake-- > 0 && stack.size() > 0)
            {
                ++count;
                nextQueue->push(std::move(stack.top()));
                stack.pop();
            }

            return count;
        }

        int try_pop_range(std::vector<T>& vec, int maxToTake, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !stack.empty(); }))
                return 0;

            int count = 0;
            while (--maxToTake >= 0 && stack.size() > 0)
            {
                vec[count] = std::move(stack.top());
                ++count;
                stack.pop();
            }

            return count;
        }

        int size()
        {
            return (int)stack.size();
        }

    protected:
        std::stack<T> stack;
        std::mutex mutex;
        std::condition_variable populatedNotifier;
    };
};