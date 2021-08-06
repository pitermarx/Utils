using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace pitermarx.Queueing
{
    public class TaskQueue
    {
        // the collection of items and the action to be performed on each one
        private readonly BlockingCollection<Expression<Action>> collection = new();

        // helpers for stopping mechanism
        private CancellationTokenSource cancelation;
        private AutoResetEvent waiter;

        /// <summary> An error method to be called if action throws </summary>
        public Action<Exception, Expression<Action>> OnError { private get; set; }

        private void DoAction(Expression<Action> action)
        {
            try
            {
                action.Compile()();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e, action);
            }
        }

        /// <summary>
        /// Always allow to add items.
        /// The queue may be stopped when adding.
        /// </summary>
        public virtual void Enqueue(Expression<Action> action)
        {
            collection.Add(action);
        }

        public void Start()
        {
            if (cancelation != null)
            {
                // already started
                return;
            }

            // create waiter and cancelation
            this.cancelation = new CancellationTokenSource();
            this.waiter = new AutoResetEvent(false);

            // create a long running task
            new Task(() =>
            {
                try
                {
                    foreach (var item in collection.GetConsumingEnumerable(cancelation.Token))
                    {
                        DoAction(item);
                    }
                }
                catch (OperationCanceledException)
                {
                }

                // on exit, set the waiter
                waiter.Set();
            }, TaskCreationOptions.LongRunning).Start();
        }

        public Expression<Action>[] Stop(bool waitForCompletion = false)
        {
            // cancel the consuing enumerable
            cancelation.Cancel();
            // wait for exit
            waiter.WaitOne();

            // cleanup
            cancelation = null;
            waiter = null;

            // if waits for all items to be completed, do it here
            if (waitForCompletion)
            {
                foreach (var item in collection.ToArray())
                {
                    DoAction(item);
                }

                return Array.Empty<Expression<Action>>();
            }

            // return remaining items
            return collection.ToArray();
        }
    }
}