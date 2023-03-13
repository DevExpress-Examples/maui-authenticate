using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.EF;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using WebApi.BusinessObjects;

namespace WebApi.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater
{
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion)
    {
    }
    public override void UpdateDatabaseAfterUpdateSchema()
    {
        base.UpdateDatabaseAfterUpdateSchema();

        //Test users are created here
        CreateUserIfDoesntExist("Alex", "123", () => GetRoleWithDefaultPermissions());
        CreateUserIfDoesntExist("Admin", "", () => CreateAdminRole());
        ObjectSpace.CommitChanges();
    }
    private ApplicationUser CreateUserIfDoesntExist(string name, string password, Func<PermissionPolicyRole> createRoleAction)
    {
        var sampleUser = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == name);
        if (sampleUser == null)
        {
            sampleUser = ObjectSpace.CreateObject<ApplicationUser>();
            sampleUser.UserName = name;
            sampleUser.SetPassword(password);
            sampleUser.Roles.Add(createRoleAction());
            ((ISecurityUserWithLoginInfo)sampleUser).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(sampleUser));
        }
        return sampleUser;
    }
    private PermissionPolicyRole GetRoleWithDefaultPermissions(string name = "Default")
    {
        var role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == name);
        if (role == null)
        {
            role = ObjectSpace.CreateObject<PermissionPolicyRole>();
            role.Name = name;
            role.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            role.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
        }
        return role;
    }

    private PermissionPolicyRole CreateAdminRole()
    {
        var role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Administrators");
        if (role == null)
        {
            role = ObjectSpace.CreateObject<PermissionPolicyRole>();
        }
        role.IsAdministrative = true;
        return role;
    }
}
