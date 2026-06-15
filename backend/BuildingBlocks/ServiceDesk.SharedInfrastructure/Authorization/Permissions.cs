namespace ServiceDesk.SharedInfrastructure.Authorization;

/// <summary>
/// The central permission catalog. The single source of permission strings used by
/// <c>[RequirePermission]</c> and the role to permission map.
/// </summary>
public static class Permissions
{
    public const string TicketsCreate = "tickets.create";
    public const string TicketsViewOwn = "tickets.view.own";
    public const string TicketsViewAll = "tickets.view.all";
    public const string TicketsAssign = "tickets.assign";
    public const string TicketsComment = "tickets.comment";
    public const string TicketsAttach = "tickets.attach";
    public const string TicketsTransition = "tickets.transition";
    public const string TicketsClose = "tickets.close";
    public const string TicketsReopen = "tickets.reopen";
    public const string TicketsDelete = "tickets.delete";
    public const string TicketsRate = "tickets.rate";
    public const string CategoriesManage = "categories.manage";
    public const string UsersManage = "users.manage";
    public const string UsersForceLogout = "users.forceLogout";
    public const string DashboardViewOwn = "dashboard.viewOwn";
    public const string DashboardViewAll = "dashboard.viewAll";
    public const string SlaManage = "sla.manage";
    public const string NotificationsRead = "notifications.read";
    public const string AuditView = "audit.view";
}
