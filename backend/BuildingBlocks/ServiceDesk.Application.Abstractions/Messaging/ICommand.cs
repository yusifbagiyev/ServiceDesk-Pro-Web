using MediatR;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Application.Abstractions.Messaging;

/// <summary>Marker for a command that returns only success/failure.</summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>Marker for a command that returns a value on success.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

/// <summary>Non-generic marker used by pipeline behaviors to detect commands (vs queries).</summary>
public interface IBaseCommand;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
