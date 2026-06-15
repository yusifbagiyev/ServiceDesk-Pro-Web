using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Catalog.Domain;

public static class CategoryErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Categories.NotFound", $"Category '{id}' was not found.");

    public static readonly Error NameTaken =
        Error.Conflict("Categories.NameTaken", "A category with this name already exists.");
}
