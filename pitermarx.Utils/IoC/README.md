### [_IoC_](https://github.com/pitermarx/Utils/tree/master/pitermarx.Utils/IoC)

_A simple Inversion of Control Container_.
The instance resolution is made by Type and by name.

#### Example
```cs
// if doesn't exist, create one
var container = Factory.GetContainer("TestContainer");

// register as instanciator
container.Register(() => new MyAwesomeObject());

// register as a named instanciator
container.Register(() => new MyAwesomeObject2(), "MySpecialObject2");

// register as singleton
var myObject = new MyAwesomeObject();
container.Register(() => myObject, "MyAwesomeSingleton");

// you may register a type by interface
var myObject2 = new MyAwesomeObject();
container.Register<IMyAwesomeObject>(() => myObject2);

// Seal the container to prevent further registers
// any register will throw after this
container.Seal();

// get the registers
// the resolution is first by type and then by name
var newAwesomeObject = container.Get<MyAwesomeObject>();
var newAwesomeObject2 = container.Get<MyAwesomeObject2>("MySpecialObject2");
var sameAsMyObject = container.Get<MyAwesomeObject>("MyAwesomeSingleton");
var sameAsMyObject2 = container.Get<IMyAwesomeObject>();

// will throw KeyNotFoundException
container.Get<IMyAwesomeObject>("SomethingNotRegistered");
container.Get<MyAwesomeObject3>();
```