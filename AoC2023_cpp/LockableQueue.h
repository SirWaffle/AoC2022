#pragma once

#include <queue>
#include <chrono>
#include <mutex>
#include <condition_variable>

namespace ThreadsNThings
{
    //data passed between worker threads
    template<class T>
    class LockableQueue
    {
    public:
        using value_type = T;

        void push(T&& val)
        {
            std::lock_guard<std::mutex> lock(mutex);
            queue.push(val);
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
                queue.push(std::move(inqueue->front()));
                inqueue->pop();
            }

            populatedNotifier.notify_all();
        }

        bool try_pop(T* item, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !queue.empty(); }))
                return false;

            *item = std::move(queue.front());
            queue.pop();

            return true;
        }

        int try_pop_range(std::queue<T>* nextQueue, int maxToTake, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !queue.empty(); }))
                return 0;

            int count = 0;
            while (maxToTake-- > 0 && queue.size() > 0)
            {
                ++count;
                nextQueue->push(std::move(queue.front()));
                queue.pop();
            }

            return count;
        }

        int try_pop_range(std::vector<T>& vec, int maxToTake, std::chrono::milliseconds timeout = 1)
        {
            std::unique_lock<std::mutex> lock(mutex);

            if (!populatedNotifier.wait_for(lock, timeout, [this] { return !queue.empty(); }))
                return 0;

            int count = 0;
            while (--maxToTake >= 0 && queue.size() > 0)
            {
                vec[count] = std::move(queue.front());
                ++count;
                queue.pop();
            }

            return count;
        }

        int size()
        {
            return (int)queue.size();
        }

    protected:
        std::queue<T> queue;
        std::mutex mutex;
        std::condition_variable populatedNotifier;
    };
};