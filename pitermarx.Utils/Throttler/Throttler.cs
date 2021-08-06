using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace pitermarx.Throttler
{
    public class Throttler<T>
    {
        /// <summary> When the list count reaches this value the Flush method is called </summary>
        private readonly int listThreshold;

        /// <summary> At most, the Flush method will be called when this amount of time passes </summary>
        private readonly TimeSpan timeThreshold;

        /// <summary> The method that will be throttled </summary>
        private readonly Action<List<T>> throttledAction;

        /// <summary> Just a locker </summary>
        private readonly object callLock = new();

        /// <summary> The list of elements to be passed to throttledAction </summary>
        private List<T> elements;

        /// <summary> The task that will be called on timeout and a cancelationTokenSource for it </summary>
        private Task runningTask;
        private CancellationTokenSource cts;

        /// <summary> An error method to be called if throttledAction throws </summary>
        public Action<Exception, List<T>> OnError { private get; set; }

        /// <summary> Constructor </summary>
        /// <param name="action">The Action to be throttled</param>
        /// <param name="maxElems">When the list count reaches this value the Flush method is called</param>
        /// <param name="timeout">At most, the Flush method will be called when this amount of time passes</param>
        public Throttler(Action<List<T>> action, int maxElems, TimeSpan timeout)
        {
            throttledAction = action;
            listThreshold = maxElems;
            timeThreshold = timeout;
            elements = new List<T>();
            ScheduleFlush();
        }

        /// <summary> Adds one element to the internal list. Flushes if the threshold is reached</summary>
        public void Call(T newElem)
        {
            lock (callLock)
            {
                elements.Add(newElem);

                if (elements.Count >= listThreshold)
                {
                    Flush();
                }
            }
        }

        /// <summary> Adds a list of elements to the internal list. Flushes if the threshold is reached</summary>
        public void Call(List<T> newElems)
        {
            lock (callLock)
            {
                elements.AddRange(newElems);

                if (elements.Count >= listThreshold)
                {
                    Flush();
                }
            }
        }

        /// <summary> Forces the throttled method to be called if the internal list is not empty
        /// The timeout for the next flush is reseted.
        /// </summary>
        public void Flush()
        {
            lock (callLock)
            {
                ScheduleFlush();

                if (elements.Count > 0)
                {
                    DoAsyncFlush(elements);

                    elements = new List<T>();
                }
            }
        }

        /// <summary> Async call of the throttledAction and error handling </summary>
        private async void DoAsyncFlush(List<T> elems)
        {
            try
            {
                await Task.Run(() => throttledAction(elems));
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex, elems);
            }
        }

        /// <summary> Schedules the next flush based on the timeThreshold </summary>
        private void ScheduleFlush()
        {
            if (cts != null)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();
            runningTask = Task.Delay(timeThreshold);
            runningTask.ContinueWith(t =>
            {
                if (!cts.IsCancellationRequested)
                {
                    Flush();
                }
            }, cts.Token);
        }
    }
}