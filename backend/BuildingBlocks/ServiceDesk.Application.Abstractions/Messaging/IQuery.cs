using MediatR;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Application.Abstractions.Messaging;

/// <summary>Marker for a query returning a value on success.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
