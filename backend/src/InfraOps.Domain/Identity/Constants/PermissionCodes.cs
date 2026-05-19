namespace InfraOps.Domain.Identity.Constants;

public static class PermissionCodes
{
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";
    public const string PreventiveTemplatesRead = "preventive.templates.read";
    public const string PreventiveTemplatesWrite = "preventive.templates.write";
    public const string PreventiveRead = "preventive.read";
    public const string PreventiveExecute = "preventive.execute";
    public const string PreventiveValidate = "preventive.validate";
    public const string EntityManage = "entity.manage";

    public static IReadOnlyCollection<string> All { get; } =
    [
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesWrite,
        InventoryRead,
        InventoryWrite,
        PreventiveTemplatesRead,
        PreventiveTemplatesWrite,
        PreventiveRead,
        PreventiveExecute,
        PreventiveValidate,
        EntityManage
    ];
}
