using InfraOps.Domain.Identity.Constants;

namespace InfraOps.Infrastructure.Persistence.Seeding;

public static class IdentitySeedData
{
    public static IReadOnlyCollection<(string Code, string Description)> Permissions { get; } =
    [
        (PermissionCodes.UsersRead, "Read users"),
        (PermissionCodes.UsersWrite, "Manage users"),
        (PermissionCodes.RolesRead, "Read roles"),
        (PermissionCodes.RolesWrite, "Manage roles"),
        (PermissionCodes.InventoryRead, "Read inventory"),
        (PermissionCodes.InventoryWrite, "Manage inventory"),
        (PermissionCodes.PreventiveTemplatesRead, "Read preventive templates"),
        (PermissionCodes.PreventiveTemplatesWrite, "Manage preventive templates"),
        (PermissionCodes.PreventiveRead, "Read preventive executions"),
        (PermissionCodes.PreventiveExecute, "Execute preventive maintenance"),
        (PermissionCodes.PreventiveValidate, "Validate preventive maintenance"),
        (PermissionCodes.EntityManage, "Manage entity types")
    ];

    public static IReadOnlyCollection<(string Name, string Description, IReadOnlyCollection<string> Permissions)> Roles { get; } =
    [
        (
            RoleNames.Admin,
            "Full platform administration",
            PermissionCodes.All),
        (
            RoleNames.Technician,
            "Operational maintenance execution",
            [
                PermissionCodes.InventoryRead,
                PermissionCodes.InventoryWrite,
                PermissionCodes.PreventiveRead,
                PermissionCodes.PreventiveExecute
            ]),
        (
            RoleNames.Validator,
            "Preventive execution validation",
            [
                PermissionCodes.InventoryRead,
                PermissionCodes.PreventiveRead,
                PermissionCodes.PreventiveValidate
            ])
    ];
}
