using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using Lib.Service.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Service.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services, Assembly? controllersAssembly)
    {
        services.AddValidatorsFromAssemblyContaining<EmailRequestValidator>();

        if (controllersAssembly != null)
        {
            services.AddValidatorsFromAssembly(controllersAssembly);
        }

        ValidatorOptions.Global.DisplayNameResolver = (_, info, expression) =>
        {
            if (expression == null)
            {
                return info?.Name;
            }

            var chain = PropertyChain.FromExpression(expression);
            return chain.Count > 0 ? chain.ToString() : info?.Name;
        };

        return services;
    }
}