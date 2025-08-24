namespace FluentAnnotationsValidator.Internals.Reflection;

internal static class TypeUtils
{
    internal static bool IsAssignableFrom(Type? declaringType, Type? type)
    {
        if (declaringType is null)
            return false;

        // A class is assignable from itself.
        if (declaringType == type)
            return true;

        // Check if the declaring type is a base class of the target type.
        if (type is not null && type.IsSubclassOf(declaringType))
            return true;

        return false;
    }
}
