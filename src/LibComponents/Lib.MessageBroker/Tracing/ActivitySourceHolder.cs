using System.Diagnostics;

namespace Lib.MessageBroker.Tracing
{
    public class ActivitySourceHolder
    {
        private bool isEnabled = false;
        private readonly ActivitySource? activitySource;

        public ActivitySourceHolder(string appName)
        {
            activitySource = new ActivitySource(appName);
        }

        internal ActivitySource? ActivitySource => isEnabled ? activitySource : null;

        internal void Enable()
        {
            isEnabled = true;
        }
    }
}
