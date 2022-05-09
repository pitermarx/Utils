using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pitermarx.Queueing;
using Xunit;

namespace pitermarx.Tests
{
    public class BlockingQueueTests
    {
        private List<int> processed;
        private int exceptionCount;

        public BlockingQueue<int> Prepare(int wait, int count, bool throwing = false)
        {
            exceptionCount = 0;
            processed = new List<int>();
            var q = new BlockingQueue<int>(id =>
            {
                var w = Stopwatch.StartNew();
                processed.Add(id);
                if (throwing)
                {
                    throw new Exception("test");
                }
                while (true)
                {
                    if (w.ElapsedMilliseconds >= wait)
                    {
                        break;
                    }
                }
            });
            Enumerable.Range(0, count).ToList().ForEach(i => q.Enqueue(i));
            q.OnError = (e, i) => exceptionCount++;
            q.Start();
            return q;
        }

        [Fact]
        public void Test1_AverageTimePerItem()
        {
            var q = Prepare(5, 1000);

            var w = Stopwatch.StartNew();
            while (w.ElapsedMilliseconds <= 500) {}

            var remaining = q.Stop();
            Assert.Equal(1000, remaining.Length + processed.Count);
            Assert.True(remaining.Length >= 800);
            Assert.True(processed.Count <= 200);
            Assert.True(processed.Count > 80);
        }

        [Fact]
        public void Test2_WaitForCompletion()
        {
            var remaining = Prepare(1, 300).Stop(true);
            Assert.Equal(300, remaining.Length + processed.Count);
            Assert.Empty(remaining);
            Assert.Equal(300, processed.Count);
        }

        [Fact]
        public void Test3_WaitForCompletion_ExceptionsSaved()
        {
            var remaining = Prepare(1, 300, true).Stop(true);
            Assert.Equal(300, remaining.Length + processed.Count);
            Assert.Empty(remaining);
            Assert.Equal(300, processed.Count);
            Assert.Equal(300, exceptionCount);
        }
    }
}