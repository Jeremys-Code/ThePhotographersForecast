namespace ThePhotographersForecast.Services
{
    public static class NavigationHelper
    {
        /// <summary>
        /// Safely navigates to a page using Shell.Current.Navigation.PushAsync.
        /// Waits until Shell.Current is ready to avoid NullReferenceException.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds for Shell.Current to be ready.</param>
        public static async Task SafePushAsync(Page page, int maxWaitMs = 2000)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var startTime = DateTime.UtcNow;

            // Wait until Shell.Current and Navigation are ready
            while ((Shell.Current?.Navigation == null) &&
                   (DateTime.UtcNow - startTime).TotalMilliseconds < maxWaitMs)
            {
                await Task.Delay(50);
            }

            if (Shell.Current?.Navigation != null)
            {
                await Shell.Current.Navigation.PushAsync(page);
            }
            else
            {
                throw new InvalidOperationException(
                    "Navigation system not ready. Ensure this is called after the app's Shell is loaded."
                );
            }
        }
    }
}
