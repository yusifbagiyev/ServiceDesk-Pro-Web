using ServiceDesk.Application.Abstractions.Persistence;

namespace ServiceDesk.Identity.Application.Abstractions;

/// <summary>
/// The Identity module's unit of work (its DbContext). A per-module interface avoids the
/// DI collision that a single shared <see cref="IUnitOfWork"/> registration would cause
/// across modules (each module's DbContext is registered under its own interface).
/// </summary>
public interface IIdentityUnitOfWork : IUnitOfWork;
