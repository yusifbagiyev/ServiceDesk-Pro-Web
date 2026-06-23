using ServiceDesk.Application.Abstractions.Persistence;

namespace ServiceDesk.Tickets.Application.Abstractions;

/// <summary>The Tickets module's unit of work (its DbContext), registered under this module-specific interface.</summary>
public interface ITicketsUnitOfWork : IUnitOfWork;
