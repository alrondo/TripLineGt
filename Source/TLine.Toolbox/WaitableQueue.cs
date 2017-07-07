using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace TripLine.Toolbox
{
    /// <summary>
    /// Waitable Queue is a queue with a limited number of ressource. 
    /// Mostly used to pass buffers between threads. 
    /// </summary>
    /// <typeparam name="T">type that is used with the queues</typeparam>
    public class WaitableQueue<T>
    {
        private Semaphore _semaphore;
        private Queue<T> _queue; 


        public WaitableQueue(int count)
        {
            _semaphore = new Semaphore(0, count);
            _queue = new Queue<T>();
        }

        public T Dequeue(TimeSpan timeout)
        {
            if (!_semaphore.WaitOne(timeout))
            {
                return default(T);
            }

            return _queue.Dequeue();
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
            _semaphore.Release();
        }
    }
}