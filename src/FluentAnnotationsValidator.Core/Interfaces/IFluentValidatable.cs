using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Core.Interfaces;

/// <summary>
/// A marker interface to include types that don't have any 
/// <see cref="ValidationAttribute"/> custom attribute applied to their members.
/// </summary>
public interface IFluentValidatable { }