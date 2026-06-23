namespace ServiceDesk.SharedInfrastructure.Authorization;

/// <summary>
/// Default permission set per role, keyed by role name (kept free of any module's enum).
/// The login flow snapshots these into the session/claims so authorization is a claim check.
/// Roles: Admin (full control) / Worker = helpdesk (creates + resolves tickets) / User = employee.
/// </summary>
public static class RolePermissions
{
    public const string AdminRole = "Admin";
    public const string WorkerRole = "Worker";
    public const string UserRole = "User";

    // Employee: opens and tracks their own tickets.
    private static readonly string[] UserPermissions =
    [
        Permissions.TicketsCreate,
        Permissions.TicketsViewOwn,
        Permissions.TicketsComment,
        Permissions.TicketsAttach,
        Permissions.TicketsRate,
        Permissions.DashboardViewOwn,
        Permissions.NotificationsRead,
    ];

    // Helpdesk / IT staff: works the whole ticket queue, but not the admin-only config
    // (no users.manage, categories.manage, sla.manage, audit.view, tickets.delete; rating is the requester's).
    private static readonly string[] WorkerPermissions =
    [
        Permissions.TicketsCreate,
        Permissions.TicketsViewOwn,
        Permissions.TicketsViewAll,
        Permissions.TicketsAssign,
        Permissions.TicketsComment,
        Permissions.TicketsAttach,
        Permissions.TicketsTransition,
        Permissions.TicketsClose,
        Permissions.TicketsReopen,
        Permissions.DashboardViewOwn,
        Permissions.DashboardViewAll,
        Permissions.NotificationsRead,
    ];

    private static readonly string[] AdminPermissions =
    [
        Permissions.TicketsCreate,
        Permissions.TicketsViewOwn,
        Permissions.TicketsViewAll,
        Permissions.TicketsAssign,
        Permissions.TicketsComment,
        Permissions.TicketsAttach,
        Permissions.TicketsTransition,
        Permissions.TicketsClose,
        Permissions.TicketsReopen,
        Permissions.TicketsDelete,
        Permissions.TicketsRate,
        Permissions.CategoriesManage,
        Permissions.UsersManage,
        Permissions.UsersForceLogout,
        Permissions.DashboardViewOwn,
        Permissions.DashboardViewAll,
        Permissions.SlaManage,
        Permissions.NotificationsRead,
        Permissions.AuditView,
    ];

    public static IReadOnlyList<string> For(string role) => role switch
    {
        AdminRole => AdminPermissions,
        WorkerRole => WorkerPermissions,
        UserRole => UserPermissions,
        _ => [],
    };
}
