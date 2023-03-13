using MAUI.Services;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace MAUI.ViewModels {
    public class LoginViewModel : BaseViewModel {
        string userName;
        string password = string.Empty;
        string errorText;
        bool hasError;
        bool isAuthInProcess;

        public LoginViewModel() {
            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
            PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();

        }

        public string UserName {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string Password {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string ErrorText {
            get => errorText;
            set => SetProperty(ref errorText, value);
        }

        public bool HasError {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }

        public bool IsAuthInProcess {
            get => isAuthInProcess;
            set {
                SetProperty(ref isAuthInProcess, value);
                OnPropertyChanged(nameof(AllowNewAuthRequests));
            }
        }
        public bool AllowNewAuthRequests {
            get { return !IsAuthInProcess; }
        }

        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }

        async void OnLoginClicked() {
            IsAuthInProcess = true;
            string response = await DataStore.Authenticate(userName, password);
            IsAuthInProcess = false;
            if (!string.IsNullOrEmpty(response)) {
                ErrorText = response;
                HasError = true;
                return;
            }
            HasError = false;
            await Navigation.NavigateToAsync<SuccessViewModel>();
        }
        async void OnSignUpClicked() {
            await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Help Message", "New users are added in the Updater.UpdateDatabaseAfterUpdateSchema method", "OK");
        }
    }
}