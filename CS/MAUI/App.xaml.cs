using MAUI.Services;
using MAUI.ViewModels;
using MAUI.Views;


namespace MAUI {
	public partial class App {
		public App() {
			InitializeComponent();
			DependencyService.Register<NavigationService>();
			DependencyService.Register<WebAPIService>();

			Routing.RegisterRoute(typeof(SuccessPage).FullName, typeof(SuccessPage));
			
			MainPage = new AppShell();
			var navigationService = DependencyService.Get<INavigationService>();
			navigationService.NavigateToAsync<LoginViewModel>(true);
		}
	}
}
