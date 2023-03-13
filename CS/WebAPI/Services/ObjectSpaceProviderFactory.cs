using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Core;

public sealed class ObjectSpaceProviderFactory : IObjectSpaceProviderFactory {
    readonly ISecurityStrategyBase security;
    readonly ITypesInfo typesInfo;
    readonly IDbContextFactory<WebApi.BusinessObjects.WebAPIEFCoreDbContext> dbFactory;

    public ObjectSpaceProviderFactory(ISecurityStrategyBase security, ITypesInfo typesInfo, IDbContextFactory<WebApi.BusinessObjects.WebAPIEFCoreDbContext> dbFactory) {
        this.security = security;
        this.typesInfo = typesInfo;
        this.dbFactory = dbFactory;
    }

    IEnumerable<IObjectSpaceProvider> IObjectSpaceProviderFactory.CreateObjectSpaceProviders() {
        yield return new SecuredEFCoreObjectSpaceProvider<WebApi.BusinessObjects.WebAPIEFCoreDbContext>((ISelectDataSecurityProvider)security, dbFactory, typesInfo);
        yield return new NonPersistentObjectSpaceProvider(typesInfo, null);
    }
}
