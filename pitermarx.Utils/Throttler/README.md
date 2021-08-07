### [_Throttler_](https://github.com/pitermarx/Utils/tree/master/pitermarx.Utils/Throttler)

_A simple Throttle class that can throttle a method with the signature Action&lt;List&lt;T&gt;&gt;_


#### Example
```cs
Action<List<int>> actionToBeThrottled = () => /* Do awesome stuff */;
var maximumElementsBeforeFlush = 100;
var automaticFlushTimeout = TimeSpan.FromMinutes(5);
var throttledAction = new Throttler<int>(actionToBeThrottled, maximumElementsBeforeFlush, automaticFlushTimeout);

// now, call the throttledAction
throttledAction.Call(new int());
throttledAction.Call(new List<int>(5));

// you can flush manually to...
throttledAction.Flush();

// if an exception occurs while calling the action, the OnError method is called
throttledAction.OnError = (exception, ints) =>
    {
        /* Example error handling
        Log.Error("An error occurred while processing values", exception);
        DoFallbackAction(ints);
        */
    };
```