using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Delegate to retrieve the conventional key aspect.
/// </summary>
/// <param name="declaringType">The declaring type.</param>
/// <param name="memberName">The property, field, or method name.</param>
/// <param name="attribute">The validation attribute.</param>
/// <returns></returns>

public delegate string ConventionalKeyDelegate(Type declaringType, string memberName, ValidationAttribute attribute);
