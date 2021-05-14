using System;

namespace Kit
{
    public static class EventHandlerExtend
    {
        public static void SafeInvoke<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs tEventArgs)
            where TEventArgs : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, tEventArgs);
            }
        }
    }
}
