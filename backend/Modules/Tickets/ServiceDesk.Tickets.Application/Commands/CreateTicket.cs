using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Inventory.Application.Contracts;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Exceptions;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record CreateTicketCommand(
    Guid CreatedByUserId,
    Guid? ReporterUserId,
    string Priority,
    string? Title,
    string? InventoryCode,
    IReadOnlyList<Guid> CategoryIds) : ICommand<Guid>;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(c => c.CreatedByUserId).NotEmpty();
        RuleFor(c => c.Priority)
            .Must(value => Enum.TryParse<TicketPriority>(value, true, out _))
            .WithMessage("Priority must be one of: Low, Normal, High, Urgent.");
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateTicketCommandHandler(
    ITicketRepository tickets,
    ISlaPolicyRepository slaPolicies,
    ITicketDirectoryReader directory,
    IInventoryLookup inventory,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<CreateTicketCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var reporterUserId = command.ReporterUserId ?? command.CreatedByUserId;

        if (command.ReporterUserId is not null)
        {
            var reporter = await directory.FindUserAsync(command.ReporterUserId.Value, cancellationToken);
            if (reporter is null || !reporter.IsActive)
            {
                return TicketErrors.UserNotFound(command.ReporterUserId.Value);
            }
        }

        string? departmentName = null;
        string? worker = null;
        string? deviceName = null;
        if (!string.IsNullOrWhiteSpace(command.InventoryCode))
        {
            var item = await inventory.GetByInventoryCodeAsync(command.InventoryCode.Trim(), cancellationToken);
            if (item is not null)
            {
                departmentName = item.DepartmentName;
                worker = item.Worker;
                deviceName = item.DeviceName;
            }
        }

        var resolved = new List<(Guid Id, string Name)>();
        foreach (var id in command.CategoryIds)
        {
            var cat = await directory.FindCategoryAsync(id, cancellationToken);
            if (cat is null || !cat.IsActive)
            {
                return TicketErrors.CategoryNotFound(id);
            }

            resolved.Add((cat.Id, cat.Name));
        }

        var priority = Enum.Parse<TicketPriority>(command.Priority, true);
        var ticket = Ticket.Create(
            now,
            priority,
            command.CreatedByUserId,
            reporterUserId,
            command.InventoryCode?.Trim(),
            departmentName,
            worker,
            deviceName,
            command.Title?.Trim());

        var policy = await slaPolicies.GetActiveByPriorityAsync(priority, cancellationToken);
        if (policy is not null)
        {
            ticket.ApplySla(policy, now);
        }

        foreach (var (id, name) in resolved)
        {
            ticket.AddCategory(id, name, now);
        }

        tickets.Add(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ticket.Id;
    }
}
