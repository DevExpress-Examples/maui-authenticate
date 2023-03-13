using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;

namespace WebApi.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891.
public class WebAPIContextInitializer : DbContextTypesInfoInitializerBase {
	protected override DbContext CreateDbContext() {
		var optionsBuilder = new DbContextOptionsBuilder<WebAPIEFCoreDbContext>()
            .UseSqlServer(";")
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new WebAPIEFCoreDbContext(optionsBuilder.Options);
	}
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class WebAPIDesignTimeDbContextFactory : IDesignTimeDbContextFactory<WebAPIEFCoreDbContext> {
	public WebAPIEFCoreDbContext CreateDbContext(string[] args) {
		throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
		//var optionsBuilder = new DbContextOptionsBuilder<WebAPIEFCoreDbContext>();
		//optionsBuilder.UseSqlServer("Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=WebAPI");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
		//return new WebAPIEFCoreDbContext(optionsBuilder.Options);
	}
}
[TypesInfoInitializer(typeof(WebAPIContextInitializer))]
public class WebAPIEFCoreDbContext : DbContext {
	public WebAPIEFCoreDbContext(DbContextOptions<WebAPIEFCoreDbContext> options) : base(options) {
	}
	public DbSet<PermissionPolicyRole> Roles { get; set; }
	public DbSet<WebApi.BusinessObjects.ApplicationUser> Users { get; set; }
    public DbSet<WebApi.BusinessObjects.ApplicationUserLoginInfo> UserLoginInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.Entity<WebApi.BusinessObjects.ApplicationUserLoginInfo>(b => {
            b.HasIndex(nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.LoginProviderName), nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.ProviderUserKey)).IsUnique();
        });
        modelBuilder.Entity<ModelDifference>()
            .HasMany(t => t.Aspects)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
