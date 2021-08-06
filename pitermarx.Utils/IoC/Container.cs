using System;
using System.Collections.Generic;
using System.Data;

namespace pitermarx.IoC
{
    public class Container
    {
        public readonly string Name;

        private readonly Dictionary<Type, Dictionary<string, Func<object>>> getters;
        private bool isSealed;

        internal Container(string instanceName)
        {
            Name = instanceName;
            getters = new Dictionary<Type, Dictionary<string, Func<object>>>();
        }

        public Container Register<T>(Func<T> getter, string getterName = "")
            where T : class
        {
            if (isSealed)
            {
                throw new ConstraintException("The IoC instance is sealed");
            }

            var dic = getters.ContainsKey(typeof(T))
                ? getters[typeof(T)]
                : (getters[typeof(T)] = new Dictionary<string, Func<object>>());
            dic[getterName ?? string.Empty] = getter;
            return this;
        }

        public T Get<T>(string getterName = "")
            => (T)getters[typeof(T)][getterName ?? string.Empty]();

        public void Seal() => isSealed = true;

        internal void Dispose()
        {
            Seal();
            getters.Clear();
        }
    }
}
