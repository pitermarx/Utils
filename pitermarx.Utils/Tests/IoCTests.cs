using System.Collections.Generic;
using System.Data;
using pitermarx.IoC;
using Xunit;

namespace pitermarx.Tests
{
    public class IoCTests
    {
        [Fact]
        public void IoCIntegrationTests()
        {
            Assert.Empty(Factory.Instances);//, "Instances starts empty");

            // if doesn't exist, create one
            var cont = Factory.GetContainer("TestContainer");

            Assert.NotEmpty(Factory.Instances);//, "TestContainer was created");

            // register two instances of the IoCTests Class. One with default name, the other with the name "2"
            cont.Register(() => this);
            cont.Register(() => new IoCTests(), "2");

            Assert.Same(cont.Get<IoCTests>(), this);//, "Default instance is 'this'");
            Assert.NotSame(cont.Get<IoCTests>("2"), this);// "Named instance is not 'this'");

            Assert.Throws<KeyNotFoundException>(() => cont.Get<IoCTests>("3"));// "Named instance '3' doesn't exist");

            // seal container to prevent further registers
            cont.Seal();

            Assert.Throws<ConstraintException>(() => cont.Register(() => this));// "Cannot register. Container is sealed");

            // assert i can still get registered instances
            Assert.Same(cont.Get<IoCTests>(), this);
            Assert.NotSame(cont.Get<IoCTests>("2"), this);
            // and the container still exists
            Assert.NotEmpty(Factory.Instances);

            // dispose container
            Factory.DisposeContainer(cont);

            Assert.Empty(Factory.Instances);// "All the containers have been disposed");
            Assert.Throws<KeyNotFoundException>(() => cont.Get<IoCTests>());// "This container has been disposed");
        }
    }
}