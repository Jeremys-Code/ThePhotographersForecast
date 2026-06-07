using ThePhotographersForecast.Views;

namespace ThePhotographersForecast
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
//            //     return Services.NavigationService.NavigateToSelectLocationAsync();
//            // }
//            // }
//            //}
//            //{
//            //    // Temporary test page to isolate AppShell issues
//            //var page = new ContentPage
//            //{
//            //   Content = new Grid
//            //   {
//            //        Children =
//            //    {
//            //        new Label { Text = "Test window", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
//            //  }
//            //}
//            //};

//            //return new Window(page);
//            //}
//            var diagnostics = new System.Collections.Generic.List<string>();
//            {
//                //  "Diagnostics: instantiate each ShellContent page to detect failing view construction.";



//                var viewTests = new (string Name, System.Func<ContentPage> Factory)[]
//                {
//                ("SelectLocation", () => new Views.SelectLocation(new ViewModels.SelectLocationViewModel())),
//                ("PhotoTimeTrack", () => new Views.PhotoTimeTrack(new ViewModels.PhotoTimeTrackViewModel()))
//                //("Forecast", () => new Views.Forecast(Services.ServiceHelper.GetService<ViewModels.ForecastViewModel>())),
//                    //  ("Calendar", () => new Views.Calendar(Services.ServiceHelper.GetService<ViewModels.CalendarViewModel>()))
//                };

//                foreach (var vt in viewTests)
//                {
//                    try
//                    {
//                        var page = vt.Factory();
//                        diagnostics.Add($"{vt.Name}: OK");
//                    }
//                    catch (System.Exception ex)
//                    {
//                        diagnostics.Add($"{vt.Name}: ERROR: {ex.GetType().Name}: {ex.Message}");
//                        System.Diagnostics.Debug.WriteLine($"View diagnostic - {vt.Name}: {ex}");
//                    }
//                }

//                var stack = new VerticalStackLayout { Padding = new Thickness(16) };
//                stack.Children.Add(new Label { Text = "View diagnostics", FontAttributes = FontAttributes.Bold, FontSize = 18 });
//                foreach (var d in diagnostics)
//                {
//                    stack.Children.Add(new Label { Text = d, FontSize = 14 });
//                }

//                stack.Children.Add(new Button
//                {
//                    Text = "Open AppShell",
//                    Command = new Command(() =>
//                    {
//                        var win = Application.Current?.Windows.FirstOrDefault();
//                        if (win != null)
//                        {
//                            win.Page = new AppShell();
//                        }
//                    })
//                });
//                stack.Children.Add(new Button
//                {
//                    Text = "Open Select Location",
//                    Command = new Command(() =>
//                    {
//                        var win = Application.Current?.Windows.FirstOrDefault();
//                        if (win != null)
//                        {
//                            var vm = Services.ServiceHelper.GetService<ViewModels.SelectLocationViewModel>();
//                            win.Page = new SelectLocation(vm);
//                        }
//                    })
//                });
//                stack.Children.Add(new Button
//                {
//                    Text = "Open PhotoTimeTrack",
//                    Command = new Command(() =>
//                    {
//                        var win = Application.Current?.Windows.FirstOrDefault();
//                        if (win != null)
//                        {
//                            var vm = Services.ServiceHelper.GetService<ViewModels.PhotoTimeTrackViewModel>();
//                            win.Page = new PhotoTimeTrack(vm);
//                        }
//                    })
//                });

//                var pageContent = new ScrollView { Content = stack };
//                var diagnosticPage = new ContentPage { Content = pageContent };
//                return new Window(diagnosticPage);
//            }
//        }
//    }
//}
