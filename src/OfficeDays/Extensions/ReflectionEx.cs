using System.Reflection;

namespace OfficeDays.Extensions;

public static class ReflectionEx
{
    extension(Assembly assembly)
    {
        public IEnumerable<Type> GetTypesImplementing<TInterface>(bool onlyClasses = true)
        {
            var interfaceType = typeof(TInterface);

            return assembly.GetTypesImplementing(interfaceType, onlyClasses);
        }
        
        public IEnumerable<Type> GetTypesImplementing(Type interfaceType, bool onlyClasses = true)
        {
            return assembly.GetExportedTypes()
                           .Where(t => (!onlyClasses || !t.IsAbstract && t.IsClass && t.IsPublic) &&
                                       (
                                           interfaceType.IsGenericType && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType) ||
                                           t.GetInterfaces().Contains(interfaceType))
                                       );
        }
    }

}
