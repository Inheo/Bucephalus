using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bucephalus.Attribute;

namespace Bucephalus.Preprocessor
{
    internal static class AsmFinder
    {

        public static IReadOnlyCollection<string> FindViewIds(IEnumerable<Type> types = null)
        {
            types ??= AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains("Unity"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<ViewIdsAttribute>(false) != null);
            
            
            var result = new HashSet<string>(64);
            foreach (var type in types)
            {
                var idsAttribute = type.GetCustomAttribute<ViewIdsAttribute>(false);
                if (idsAttribute == null)
                {
                    continue;
                }

                var names = Enum.GetNames(type);
                foreach (var name in names)
                {
                    if (!result.Add(name))
                    {
                        throw new Exception($"Duplicate UI name definition: {name}");
                    }
                }
            }

            return result;
   
        }
    }
}