#pragma once

#include "LockableQueue.h"
#include <queue>
#include <functional>
#include <iostream>

namespace ThreadsNThings
{
    template<typename InputLockableContainerType, typename OutputLockableContainerType>
    class ThreadQueueWorker
    {
    protected:
        using work_item_type = InputLockableContainerType::value_type;
        using work_item_completed_type = OutputLockableContainerType::value_type;

        std::stop_source m_stop;
        InputLockableContainerType* m_inputLockableContainer;
        OutputLockableContainerType* m_outputLockableContainer;
        uint32_t m_readChunkSize = 1024;

        uint32_t m_processedItems = 0;
        bool m_isBusy = false;

    public:
        ThreadQueueWorker(InputLockableContainerType* inputLockableContainer, OutputLockableContainerType* outputLockableContainer, uint32_t readChunkSize = 1024)
            :m_readChunkSize(readChunkSize),
            m_inputLockableContainer(inputLockableContainer),
            m_outputLockableContainer(outputLockableContainer)
        {
        }

        uint32_t GetProcessedItemCount()
        {
            return m_processedItems;
        }

        void WaitForFinish()
        {
            m_stop.request_stop();
        }

        bool IsBusy()
        {
            return m_isBusy;
        }

        void EnterProcFunc(std::function<void(work_item_type& item, std::queue<work_item_completed_type>& completedWork)> workFunc)
        {
            std::queue<work_item_completed_type> workOutQueue;
            std::queue<work_item_type> workQueue;
            work_item_type workItem;

            while (!m_stop.stop_requested() || m_inputLockableContainer->size() > 0)
            {
                if (m_inputLockableContainer->try_pop_range(&workQueue, m_readChunkSize, std::chrono::milliseconds(10000)) == 0)
                {
                    std::this_thread::sleep_for(std::chrono::milliseconds(50));
                    continue;
                }

                m_isBusy = true;

                {
                    //auto tid = std::this_thread::get_id();
                    //std::cout << "[ Thread " << tid << " ] Grabbed work of size: " << workQueue.size() << std::endl;
                }

                while (workQueue.size() > 0)
                {
                    ++m_processedItems;

                    workItem = workQueue.front();
                    workQueue.pop();

                    workFunc(workItem, workOutQueue);
                }

                m_outputLockableContainer->push_queue(&workOutQueue);

                m_isBusy = false;
            }

            //in case we have pending output on exit
            m_outputLockableContainer->push_queue(&workOutQueue);
        }
    };
};