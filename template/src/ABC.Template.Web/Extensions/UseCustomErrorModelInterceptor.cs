using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Primitives;
using Newtonsoft.Json;

namespace ABC.Template.Web.Extensions;

public class UseCustomErrorModelInterceptor : IValidatorInterceptor
{
    public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
    {
        return commonContext;
    }

    public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext,
        ValidationResult result)
    {
        if (result.Errors == null || result.Errors.Count == 0)
            return result;
        throw new KnownException(message: result.Errors[0].ErrorMessage, errorCode: 400, errorData: result.Errors
            .Select(p =>
                new { errorCode = p.ErrorCode, errorMessage = p.ErrorMessage, propertyName = p.PropertyName }
            ).ToArray());
    }
}