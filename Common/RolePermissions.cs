using System.Security;

namespace BotTrungThuong.Common
{
    public static class RolePermissions
    {
        private static readonly Permission allPermissions = Permission.View | Permission.Edit | Permission.Delete | Permission.Create;

        public static readonly Dictionary<string, Dictionary<UserRole, Permission>> PermissionsByController = new()
        {
            ["User"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Employee, Permission.None },    
                { UserRole.Manager, Permission.None },     
                { UserRole.Admin, allPermissions },
                { UserRole.Assistant, Permission.None }      
            },
            ["ThietLapTrungThuong"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Employee, Permission.None },
                { UserRole.Manager, Permission.View | Permission.Edit | Permission.Delete | Permission.Create },
                { UserRole.Admin, allPermissions},
                { UserRole.Assistant, Permission.None }
            },
            ["DanhSachTrungThuong"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Employee, Permission.None },
                { UserRole.Manager, Permission.View | Permission.Edit | Permission.Delete | Permission.Create },
                { UserRole.Admin, allPermissions},
                { UserRole.Assistant, Permission.None }
            },
            ["TeleBot"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Employee, Permission.None },
                { UserRole.Manager, Permission.View | Permission.Edit | Permission.Delete | Permission.Create },
                { UserRole.Admin, allPermissions },
                { UserRole.Assistant, Permission.None }
            },
            ["GiaiThuong"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Employee, Permission.None },
                { UserRole.Manager, Permission.View | Permission.Edit | Permission.Delete | Permission.Create },
                { UserRole.Admin, allPermissions},
                { UserRole.Assistant, Permission.None }
            },
        };

        public static Permission GetPermissions(string controllerName, UserRole role)
        {
            if (PermissionsByController.TryGetValue(controllerName, out var rolePermissions))
            {
                return rolePermissions.TryGetValue(role, out var permission) ? permission : Permission.None;
            }

            return Permission.None;
        }
    }
}
