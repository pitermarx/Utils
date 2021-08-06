using System;
using System.Collections.Concurrent;
using System.Reflection.Emit;

namespace pitermarx.Reflection
{
    public static class FastActivator
    {
        private static readonly Type ObjectType = typeof(object);
        private static readonly Type ConstructorType = typeof(Func<object>);

        private static readonly ConcurrentDictionary<Type, Func<object>> ConstructorsCache = new();

        public static object New<T>()
        {
            return (T)New(typeof(T));
        }

        public static object New(Type type)
        {
            try
            {
                return ConstructorsCache.GetOrAdd(type, CreateConstructor)();
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("FastActivator couldn't create instance for type '{0}' from assemebly '{1}'",
                    type.FullName, type.AssemblyQualifiedName), exc);
            }
        }

        private static Func<object> CreateConstructor(Type type)
        {
            if (type.IsClass)
            {
                // classes
                var dynMethod = new DynamicMethod("_", type, null);
                var ilGen = dynMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Ret);
                return (Func<object>)dynMethod.CreateDelegate(ConstructorType);
            }
            else
            {
                // structs
                var dynMethod = new DynamicMethod("_", ObjectType, null);
                var ilGen = dynMethod.GetILGenerator();
                var lv = ilGen.DeclareLocal(type);
                ilGen.Emit(OpCodes.Ldloca_S, lv);
                ilGen.Emit(OpCodes.Initobj, type);
                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Box, type);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object>)dynMethod.CreateDelegate(ConstructorType);
            }
        }
    }
}