using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// A lightweight descriptor stub for attribute-based validators.
/// No rule composition metadata is exposed.
/// </summary>
public sealed class FluentValidatorDescriptor : IValidatorDescriptor
{
    public IEnumerable<IValidationRule> Rules => [];

    public string GetName(string property) => property;

    public ILookup<string, (IPropertyValidator Validator, IRuleComponent Options)> GetMembersWithValidators() =>
        Enumerable.Empty<(IPropertyValidator, IRuleComponent)>().ToLookup(_ => string.Empty);

    public IEnumerable<(IPropertyValidator Validator, IRuleComponent Options)> GetValidatorsForMember(string name) => [];

    public IEnumerable<IValidationRule> GetRulesForMember(string name) => [];
}
