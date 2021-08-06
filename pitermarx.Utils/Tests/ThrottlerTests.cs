using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoFixture;
using pitermarx.Throttler;
using Xunit;

namespace pitermarx.Tests
{
    public class ThrottlerTests
    {
        private readonly AutoResetEvent waiter = new(false);
        private int length = 0;
        private void Reseter(List<int> list)
        {
            length = list.Count;
            waiter.Set();
        }


        [Fact]
        public void ThrottledMetthodIsNotCalledOnEmptyQueue()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMinutes(5));

            // test
            throttled.Flush();

            // assert
            Assert.False(waiter.WaitOne(2000));
        }

        [Fact]
        public void ThrottledMetthodIsNotCalled()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMinutes(5));

            // test
            throttled.Call(new int());
            throttled.Call(new List<int>(5));

            // assert
            Assert.False(waiter.WaitOne(2000));
        }

        [Fact]
        public void ThrottledMetthodIsCalledOnFlush()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMinutes(5));

            // test
            throttled.Call(new int());
            throttled.Call(new int());
            throttled.Call(new int());
            throttled.Call(new List<int>());
            throttled.Flush();

            // assert
            Assert.True(waiter.WaitOne(2000));
            Assert.True(length == 3);
        }

        [Fact]
        public void ThrottledMetthodIsCalledOnTimeout()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMilliseconds(200));

            // test
            throttled.Call(new int());
            throttled.Call(new List<int>());

            // assert
            Assert.True(waiter.WaitOne(2000));
            Assert.True(length == 1);
            Assert.False(waiter.WaitOne(2000));
        }

        [Fact]
        public void ThrottledMetthodIsCalledOnTimeout2()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMilliseconds(200));

            // test
            throttled.Call(new int());
            throttled.Call(new List<int>());

            // assert
            Assert.True(waiter.WaitOne(2000));
            Assert.True(length == 1);

            // just one more
            throttled.Call(new int());
            Assert.True(waiter.WaitOne(2000));
        }

        [Fact]
        public void ThrottledMetthodIsCalledOnQueueFull()
        {
            // arrange
            var throttled = new Throttler<int>(Reseter, 100, TimeSpan.FromMinutes(5));
            throttled.Call(new Fixture().CreateMany<int>(99).ToList());

            // assert 1
            Assert.False(waiter.WaitOne(2000));

            // test
            throttled.Call(new int());

            // assert 2
            Assert.True(waiter.WaitOne(2000));
            Assert.True(length == 100);
        }

        [Fact]
        public void ThrottledMetthodThrows()
        {
            // arrange
            var throttled = new Throttler<int>(list => { throw new Exception("ERROR"); }, 100, TimeSpan.FromMilliseconds(100))
            {
                OnError = (ex, list) => waiter.Set()
            };

            throttled.Call(new int());

            // assert 1
            Assert.True(waiter.WaitOne(2000));
        }
    }
}
