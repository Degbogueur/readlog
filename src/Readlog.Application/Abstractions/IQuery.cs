using MediatR;
using Readlog.Application.Shared;

namespace Readlog.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
