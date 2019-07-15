using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesis
{
    class ManagedTaskRunner
    {
        public static Thread Create(ManagedTask task)
        {
            Thread t = new Thread(() =>
            {
                task.OnStart();
                while (true)
                {
                    task.OnMessage();
                    if (!task.IsAlive())
                    {
                        return;
                    }
                    if (task.IsPaused())
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        task.OnCycle();
                    }
                }
            });
            return t;
        }
    }
}
