using Prism;
using Prism.Ioc;
using Prism.Unity;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace App1
{
    public partial class App
    {

        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnInitialized()
        {
            
            NavigationService.NavigateAsync("MainPage");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>("MainPage");

        }


    }
}