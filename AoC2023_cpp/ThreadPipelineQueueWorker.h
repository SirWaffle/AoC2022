#pragma once

#include "LockableQueue.h"
#include <queue>
#include <functional>

namespace ThreadsNThings
{
    template<typename InputDataType, typename OutputDataType>
    class ThreadPipelineQueueWorker
    {
    protected:
        std::stop_source m_stop;
        LockableQueue<InputDataType*>* m_inputQueue;
        LockableQueue<OutputDataType*>* m_outputQueue;
        uint32_t m_workOutQueueSize = 1024;
        uint32_t m_readChunkSize = 1024;


    public:
        ThreadPipelineQueueWorker(LockableQueue<InputDataType*>* inputQueue, LockableQueue<OutputDataType*>* outputQueue, uint32_t readChunkSize = 1024, uint32_t workOutQueueSize = 1024)
            :m_readChunkSize(readChunkSize),
            m_workOutQueueSize(workOutQueueSize),
            m_inputQueue(inputQueue),
            m_outputQueue(outputQueue)
        {
        }

        void WaitForFinish()
        {
            m_stop.request_stop();
        }

        LockableQueue< OutputDataType* >* GetOutputQueuePtr()
        {
            return m_outputQueue;
        }

        void EnterProcFunc(std::function<OutputDataType* (InputDataType*)> workFunc)
        {
            std::queue<OutputDataType*> workOutQueue;
            std::queue<InputDataType*> workQueue;
            InputDataType* workItem;

            while (!m_stop.stop_requested() || m_inputQueue->Length() > 0)
            {
                if (m_inputQueue->try_pop_range(&workQueue, m_readChunkSize, 10000ms) == 0)
                {
                    std::this_thread::sleep_for(50ms);
                    continue;
                }

                while (workQueue.size() > 0)
                {
                    //loop over the batchQueueIn, grab some items, hash em, stuff em in the hashedDataQueue
                    workItem = workQueue.front();
                    workQueue.pop();

                    OutputDataType* outData = workFunc(workItem);

                    workOutQueue.push(std::move(outData));

                    if (m_workOutQueueSize < workOutQueue.size())
                        m_outputQueue->push_queue(&workOutQueue);
                }
            }

            if (workOutQueue.size() > 0)
                m_outputQueue->push_queue(&workOutQueue);
        }
    };
};