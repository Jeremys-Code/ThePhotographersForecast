#if ANDROID
using Android.Content;
using Android.Runtime;
using Android.Views;
using ThePhotographersForecast.Views;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services;
#elif IOS
using ThePhotographersForecast.Views;
using ThePhotographersForecast.Services;
using UIKit;
#endif


namespace ThePhotographersForecast.Services
{
    public class DeviceOrientationService
    {
        public enum DeviceOrientation
        {
            Undefined,
            Portrait,
            Landscape
        }
        public DeviceOrientation GetOrientation()
        {
#if ANDROID
            IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            SurfaceOrientation orientation = windowManager.DefaultDisplay.Rotation;
            bool isLandscape = orientation == SurfaceOrientation.Rotation90 || orientation == SurfaceOrientation.Rotation270;
            return isLandscape ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
#elif IOS
            UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
            bool isPortrait = orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
            return isPortrait ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
#else
            return DeviceOrientation.Undefined;
#endif
        }
    }
}