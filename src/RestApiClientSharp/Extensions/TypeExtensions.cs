using System.Reflection;
using System.Runtime.CompilerServices;

namespace AndreasReitberger.API.REST.Extensions
{
    [Obsolete("Use from the core library once available")]
    public static class TypeExtensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                return false;

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType
                   && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

    }
}
