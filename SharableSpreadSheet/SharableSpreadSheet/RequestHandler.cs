using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace shareableSpreadSheet
{
    class RequestHandler
    {

        Mutex handler;
        int requests_Int;

        public RequestHandler()
        {
            this.handler = new Mutex();
            requests_Int = 0;
        }

        public void Request()
        {
            Interlocked.Increment(ref requests_Int);
            handler.WaitOne();
            Console.WriteLine("Thread Entered Protected area {0} ", Thread.CurrentThread.ManagedThreadId);
        }

        public void done()
        {
            Interlocked.Decrement(ref requests_Int);
            handler.ReleaseMutex();
            Console.WriteLine("Thread Exited Protected area {0} ", Thread.CurrentThread.ManagedThreadId);
        }


    }
}
