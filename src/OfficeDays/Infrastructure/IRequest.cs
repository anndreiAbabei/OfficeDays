namespace OfficeDays.Infrastructure;

public interface IRequest;

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    ValueTask<IResult> Handle(TRequest request, CancellationToken cancellationToken);
}