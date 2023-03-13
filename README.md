# Authenticate Users with the WebAPI Service

Our free [.NET App Security Library](https://www.devexpress.com/products/net/application_framework/security-web-api-service.xml) includes a wizard that generates a ready-to-use authentication service. This service uses the [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) ORM to access a database. 

You can find more information about our WebAPI Service's access restrictions in the following topic and video:

[Create a Standalone Web API Application](https://docs.devexpress.com/eXpressAppFramework/403401/backend-web-api-service/create-new-application-with-web-api-service?p=net6)

[A 1-Click Solution for CRUD Web API with Role-based Access Control via EF Core & ASP.NET](https://www.youtube.com/watch?v=T7y4gwc1n4w&list=PL8h4jt35t1wiM1IOux04-8DiofuMEB33G)


## Run Projects

Perform the following steps to run the project:

1. Open the `WebAPI` solution in the Visual Studio as an administrator. This is required to create the database whose settings are defined in the `appsettings.json` file.

2. Choose the `WebApi` item in the **debug** dropdown menu. From this menu, you can debug the project on the [Kestrel](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-7.0) web server.

    ![Run Settings](images/authenticate-run-settings@2x.png)

    If you prefer **IIS Express** instead of `Kestrel`, you need to select the IIS Express on the **debug** dropdown menu, open the `.vs\MAUI_WebAPI\config\applicationhost.config` in an external text editor, and add the following code:

    ```xaml
    <sites>
        <site name="WebSite1" id="1" serverAutoStart="true">
        <!-* ... -->
            <bindings>
                <binding protocol="http" bindingInformation="*:65201:*" />
                <binding protocol="https" bindingInformation="*:44317:*" />
                <binding protocol="https" bindingInformation="*:44317:localhost" />
                <binding protocol="http" bindingInformation="*:65201:localhost" />
            </bindings>
        </site>
        <!-* ... -->
    </sites>
    ```

3. Right-click the MAUI project, choose `Set as Startup Project` and select your emulator. Note that physical devices that are attached over USB do not allow you to access your machine's localhost.
4. Right-click the `WebAPI` project and select `Debug > Start new instance` to run the `WebAPI` project.
5. Right-click the `MAUI` project and select `Debug > Start new instance` to run the `MAUI` project.

## Implementation Details

### Service and Communication

* The WebAPI service uses json web tokens (JWT) to authorize users. Call `WebAPI`'s **Authenticate** endpoint and pass a username and password to the endpoint from the .NET MAUI application. In the example, this logic is implemented in the `WebAPIService.RequestTokenAsync` method:
  
    ```csharp
      private async Task<HttpResponseMessage> RequestTokenAsync(string userName, string password) {
            return await HttpClient.PostAsync($"{ApiUrl}Authentication/Authenticate",
                                                new StringContent(JsonSerializer.Serialize(new { userName, password = $"{password}" }), Encoding.UTF8,
                                                ApplicationJson));
      }
    ```

    Include the token in request’s [HttpClient.DefaultRequestHeaders.Authorization](xref:System.Net.Http.HttpClient.DefaultRequestHeaders) header to allow all subsequent requests access private endoints and data: 

    ```csharp
    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenResponse.Content.ReadAsStringAsync());
    ```

  File to Look At: [WebAPIService.cs](CS/MAUI/Services/WebAPIService.cs)

* To create users and specify their passwords, use the `Updater.UpdateDatabaseAfterUpdateSchema` method. You can modify a user's password directly in the database or use the full version of our [XAF UI](xref:112649#end-user-password-modifications).

    File to Look At: [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)

### Login UI and View Model

* Use the [TextEdit.StartIcon](xref:DevExpress.Maui.Editors.EditBase.StartIcon) and [PasswordEdit.StartIcon](xref:DevExpress.Maui.Editors.EditBase.StartIcon) properties to display icons in the [TextEdit](xref:DevExpress.Maui.Editors.TextEdit) and [PasswordEdit](xref:DevExpress.Maui.Editors.PasswordEdit) controls.

    ```xaml
    <dxe:TextEdit LabelText="Login" StartIcon="editorsname" .../>
    <dxe:PasswordEdit LabelText="Password" StartIcon="editorspassword" .../>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)

* To validate the [PasswordEdit](xref:DevExpress.Maui.Editors.PasswordEdit) control's value, use the [EditBase.HasError](xref:DevExpress.Maui.Editors.EditBase.HasError) and [EditBase.ErrorText](xref:DevExpress.Maui.Editors.EditBase.ErrorText) inherited properties.

    ```xaml
    <dxe:PasswordEdit ... HasError="{Binding HasError}" ErrorText="{Binding ErrorText}"/>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)

    ```csharp
    public class LoginViewModel : BaseViewModel {
        // ...
        string errorText;
        bool hasError;
        // ...

        public string ErrorText {
            get => errorText;
            set => SetProperty(ref errorText, value);
        }

        public bool HasError {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }
        async void OnLoginClicked() {
            /// ...
            string response = await DataStore.Authenticate(userName, password);
            if (!string.IsNullOrEmpty(response)) {
                ErrorText = response;
                HasError = true;
                return;
            }
            HasError = false;
            await Navigation.NavigateToAsync<SuccessViewModel>();
        }
    }
    ```

    File to Look At: [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)


* Specify the [TextEdit.ReturnType](xref:DevExpress.Maui.Editors.EditBase.ReturnType) inherited property to focus the [PasswordEdit](xref:DevExpress.Maui.Editors.PasswordEdit) control after the [TextEdit](xref:DevExpress.Maui.Editors.TextEdit) control's value is edited.
* Bind the [PasswordEdit.ReturnCommand](xref:DevExpress.Maui.Editors.EditBase.ReturnCommand) property to the **Login** command to execute the command when you enter the password:

    ```xaml
    <dxe:PasswordEdit ReturnCommand="{Binding LoginCommand}"/>
    ```

    File to Look At: [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)
    
    ```csharp
    public class LoginViewModel : BaseViewModel {
        // ...
        public LoginViewModel() {
            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
            PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

        }
        // ...
        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }
        // ...
    }
    ```

    File to Look At: [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)


### Debug Specifics

Android emulator and iOS simulator request a certificate to access a service over HTTPS. In this example, we switch to HTTP in debug mode:

```csharp
#if !DEBUG
    app.UseHttpsRedirection();
#endif
```

#### MAUI - Android

```xml
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">10.0.2.2</domain>
    </domain-config>
</network-security-config>
```

#### MAUI - iOS

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsLocalNetworking</key>
    <true/>
</dict>
```

This allows you to bypass the certificate check and avoid creating a development certificate and implementing HttpClient handlers.

For more information, please refer to [Connect to local web services from Android emulators and iOS simulators](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-7.0#android-network-security-configuration).

**We highly recommend using HTTP only for debug and development purposes. In production, use HTTPS for security reasons.**

## Files to Look At

* [WebAPIService.cs](CS/MAUI/Services/WebAPIService.cs)
* [Updater.cs](CS/WebAPI/DatabaseUpdate/Updater.cs)
* [LoginPage.xaml](CS/MAUI/Views/LoginPage.xaml)
* [LoginViewModel.cs](CS/MAUI/ViewModels/LoginViewModel.cs)

## Documentation

* [Featured Scenario: Authenticate](https://docs.devexpress.com/MAUI/404316)
* [Featured Scenarios](https://docs.devexpress.com/MAUI/404291)
* [Create a Standalone Web API Application](https://docs.devexpress.com/eXpressAppFramework/403401/backend-web-api-service/create-new-application-with-web-api-service?p=net6)

## More Examples

* [DevExpress Mobile UI for .NET MAUI](https://github.com/DevExpress-Examples/maui-demo-app/)
