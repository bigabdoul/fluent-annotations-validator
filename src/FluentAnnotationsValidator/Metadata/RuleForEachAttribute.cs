using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that validation rules should be applied to each element of a collection.
/// </summary>
/// <typeparam name="TElement">The type of the elements in the collection.</typeparam>
public class RuleForEachAttribute<TElement> : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuleForEachAttribute{TElement}"/> class.
    /// </summary>
    public RuleForEachAttribute()
    {
    }
}
