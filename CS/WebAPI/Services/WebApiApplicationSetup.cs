using DevExpress.ExpressApp;
using DevExpress.ExpressApp.AspNetCore;
using DevExpress.ExpressApp.AspNetCore.WebApi;
using DevExpress.Persistent.BaseImpl.EF;

namespace WebApi.Core;

public class WebApiApplicationSetup : IWebApiApplicationSetup {
    public void SetupApplication(AspNetCoreApplication application) {
        application.Modules.Add(new WebApi.WebAPIModule());

            application.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        //application.DatabaseVersionMismatch += (s, e) => {
        //    e.Updater.Update();
        //    e.Handled = true;
        //};
    }
}
