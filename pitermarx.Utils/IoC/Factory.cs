using System.Collections.Generic;

namespace pitermarx.IoC
{
    public static class Factory
    {
        internal static Dictionary<string, Container> Instances = new();

        public static Container GetContainer(string name)
        {
            if (!Instances.ContainsKey(name))
            {
                return Instances[name] = new Container(name);
            }

            return Instances[name];
        }

        public static void DisposeContainer(Container instance)
        {
            DisposeContainer(instance.Name);
        }

        public static void DisposeContainer(string name)
        {
            if (Instances.ContainsKey(name))
            {
                Instances[name].Dispose();
                Instances.Remove(name);
            }
        }
    }
}