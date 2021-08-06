using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoFixture;
using pitermarx.Throttler;
using pitermarx.Utils;
using Xunit;

namespace pitermarx.Tests
{
    public class ShortGuidTests
    {
        [Fact]
        public void Test()
        {
            Assert.True(new ShortGuid(Guid.Empty) == (ShortGuid)Guid.Empty);
            Assert.True((Guid)ShortGuid.Empty == Guid.Empty);

            var guid = Guid.NewGuid();
            var shortGuid = new ShortGuid(guid);

            Assert.True(shortGuid == (ShortGuid)guid);
            Assert.True(shortGuid.Equals(guid));
            Assert.True(shortGuid == ShortGuid.Parse(shortGuid.ToString()));
            Assert.True(shortGuid.Equals(shortGuid.ToString()));
            Assert.True(shortGuid.GetHashCode() == guid.GetHashCode());

            shortGuid = ShortGuid.NewGuid();
            guid = (Guid)shortGuid;

            Assert.True(shortGuid == (ShortGuid)guid);
            Assert.True(shortGuid.Equals(guid));
            Assert.True(shortGuid == ShortGuid.Parse(shortGuid.ToString()));
            Assert.True(shortGuid.Equals(shortGuid.ToString()));
            Assert.True(shortGuid.GetHashCode() == guid.GetHashCode());

            Assert.True(ShortGuid.NewGuid() != ShortGuid.NewGuid());
        }
    }
}
