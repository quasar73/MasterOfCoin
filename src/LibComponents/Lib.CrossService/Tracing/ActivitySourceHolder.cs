using System.Diagnostics;

namespace Lib.CrossService.Tracing
{
    public class ActivitySourceHolder
    {
        private bool isEnabled = false;
        private readonly ActivitySource? _activitySource;

        public ActivitySourceHolder(ActivitySource? activitySource)
        {
            _activitySource = activitySource;
        }

        internal ActivitySource? GetActivitySource()
        {
            return isEnabled ? _activitySource : null;
        }

        internal void Enable()
        {
            isEnabled = true;
        }
    }
}
