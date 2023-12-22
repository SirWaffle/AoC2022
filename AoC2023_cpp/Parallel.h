/*
namespace Parallel
{

    template <class Iter, class Func>
    void parallel_for_each(unsigned threadCount, Iter first, Iter last, Func func)
    {
        Iter it = first;
        if (it == last)
            return;
        if (++it == last)
        {
            func(*first);
            return;
        }

        if (threadCount == 0)
            threadCount = std::max(2u, std::thread::hardware_concurrency());

        std::mutex mx;
        std::vector<std::thread> threads;
        threads.reserve(threadCount - 1);

        auto func2 = [&]() {
            for (;;)
            {
                Iter it;
                {
                    std::lock_guard<std::mutex> lock(mx);
                    it = first;
                    if (it == last)
                        break;
                    ++first;
                }
                func(*it);
            }
            };
        for (unsigned i = 0; i < threadCount - 1; ++i, ++it)
        {
            if (it == last)
                break;
            threads.emplace_back(std::thread(func2));
        }
        func2();
        for (auto& th : threads)
            th.join();
    }

    template <class Iter, class Func>
    void parallel_for_each(Iter first, Iter last, Func func)
    {
        parallel_for_each(std::thread::hardware_concurrency(), first, last, func);
    }
}*/