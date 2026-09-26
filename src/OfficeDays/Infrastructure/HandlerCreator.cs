using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace OfficeDays.Infrastructure;

public interface IHandlerCreator
{
    ValueTask<IResult> Create<TRequest>([AsParameters] TRequest request, 
                                        [FromServices] IRequestHandler<TRequest> handler,
                                        [FromServices] ILogger<TRequest> logger,
                                        [FromServices] IValidator<TRequest>? validator = null,
                                        CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}

public sealed partial class HandlerCreator : IHandlerCreator
{
    public async ValueTask<IResult> Create<TRequest>([AsParameters] TRequest request, 
                                                     [FromServices] IRequestHandler<TRequest> handler,
                                                     [FromServices] ILogger<TRequest> logger,
                                                     [FromServices] IValidator<TRequest>? validator = null,
                                                     CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        LogStartRequest(logger, request);
        var sw = Stopwatch.StartNew();
        
        if(validator != null)
        {
            LogStartValidatingRequest(logger);
            
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            var elapsed = sw.Elapsed;
            
            if (!validationResult.IsValid)
            {
                LogValidationFailure(logger, validationResult.Errors, elapsed);
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            
            LogValidatingPass(logger, elapsed);
        }
        
        var result = await handler.Handle(request, cancellationToken);
        
        sw.Stop();
        
        LogFinishedRequest(logger, result, sw.Elapsed);
        
        return result;
    }

    private const int EventIdStart = 0100;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Debug, "Start request {Request}")]
    private static partial void LogStartRequest(ILogger logger, object request);
    
    [LoggerMessage(EventIdStart + 2, LogLevel.Debug, "Start validating request")]
    private static partial void LogStartValidatingRequest(ILogger logger);
    
    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "Validation for the request failed with {Failures}, took {Elapsed}")]
    private static partial void LogValidationFailure(ILogger logger, List<ValidationFailure> failures, TimeSpan elapsed);
    
    [LoggerMessage(EventIdStart + 4, LogLevel.Debug, "Validation for the request has passed, took {Elapsed}")]
    private static partial void LogValidatingPass(ILogger logger, TimeSpan elapsed);
    
    [LoggerMessage(EventIdStart + 5, LogLevel.Debug, "Request finished with result {Result}, took {Elapsed}")]
    private static partial void LogFinishedRequest(ILogger logger, IResult result, TimeSpan elapsed);
}
