using FluentAnnotationsValidator.Configuration;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FluentAnnotationsValidator.Messages;

/// <summary>
/// Represents the result of an instance of <see cref="IStringLocalizerFactory"/> configuration through DI.
/// </summary>
/// <param name="SharedResourceType">
/// The type of the shared resource. If set, it is assigned to the 
/// <see cref="ValidationBehaviorOptions.SharedResourceType"/> 
/// property value, if that latter hasn't been set yet.
/// </param>
/// <param name="SharedCulture">
/// The culture of the shared resource. If set, it is assigned to the
/// <see cref="ValidationBehaviorOptions.SharedCulture"/> property,
/// if that latter hasn't been set yet.
/// </param>
public record StringLocalizerFactoryResult(Type? SharedResourceType = null, CultureInfo? SharedCulture = null);