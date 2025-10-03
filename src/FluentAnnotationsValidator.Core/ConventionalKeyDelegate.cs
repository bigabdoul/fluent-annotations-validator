using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Core;

/// <summary>
/// Delegate to retrieve the conventional resource key aspect.
/// </summary>
/// <remarks>
/// A conventional resource key follows the pattern <c>Property_Attribute</c>.
/// For instance, a class with an Email property decorated with the
/// <c>[Required]</c>, <c>[EmailAddress]</c>, and <c>[StringLength(100)]</c> 
/// custom attributes has the <c>Email_Required</c>, <c>Email_EmailAddress</c>, 
/// and <c>Email_StringLength</c> conventional resource key names, and 
/// (by default) localized strings will be looked up with these keys.
/// <para>
/// This delegate is an extensibility point allowing you to change this behavior.
/// </para>
/// </remarks>
/// <param name="objectType">The object type.</param>
/// <param name="memberName">The property, field, or method name.</param>
/// <param name="attribute">The validation attribute.</param>
/// <returns>A string representing the conventional key aspect.</returns>
public delegate string ConventionalKeyDelegate(Type objectType, string memberName, ValidationAttribute attribute);
