using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Defines a delegate that provides a new value for a specified member before validation occurs.
/// This can be used to perform pre-validation value gathering or transformation.
/// </summary>
/// <param name="instance">The object instance being validated.</param>
/// <param name="member">The <see cref="MemberInfo"/> of the member being validated.</param>
/// <param name="memberValue">The current value of the member.</param>
/// <returns>
/// The new value for the member, or the original <paramref name="memberValue"/> if no change is needed.
/// </returns>
public delegate object? PreValidationValueProviderDelegate(object instance, MemberInfo member, object? memberValue);

/// <summary>
/// Defines a type-specific delegate that provides a new value for a specified member before validation occurs.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <param name="instance">The object instance being validated.</param>
/// <param name="member">The <see cref="MemberInfo"/> of the member being validated.</param>
/// <param name="memberValue">The current value of the member.</param>
/// <returns>
/// The new value for the member, or the original <paramref name="memberValue"/> if no change is needed.
/// </returns>
public delegate object? PreValidationValueProviderDelegate<T>(T instance, MemberInfo member, object? memberValue);

/// <summary>
/// Defines a type-safe delegate that provides a new value for a specified member before validation occurs.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
/// <param name="instance">The object instance being validated.</param>
/// <param name="member">The <see cref="MemberInfo"/> of the member being validated.</param>
/// <param name="memberValue">The current value of the member.</param>
/// <returns>
/// The new value for the member, or the original <paramref name="memberValue"/> if no change is needed.
/// </returns>
public delegate TProp? PreValidationValueProviderDelegate<T, TProp>(T instance, MemberInfo member, TProp? memberValue);