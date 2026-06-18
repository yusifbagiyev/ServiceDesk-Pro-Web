using ServiceDesk.Application.Abstractions.Persistence;

namespace ServiceDesk.Catalog.Application.Abstractions;

/// <summary>The Catalog module's unit of work (its DbContext), registered under this module-specific interface.</summary>
public interface ICatalogUnitOfWork : IUnitOfWork;
