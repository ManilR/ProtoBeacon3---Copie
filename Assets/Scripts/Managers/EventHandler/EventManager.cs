using System.Collections.Generic;

namespace Beacon
{
    public class EventManager
    {
        public static EventManager Instance
        {
            get { return instance == null ? instance = new EventManager() : instance; }
        }
        private static EventManager instance = null;

        public delegate void EventDelegate<T>(T e) where T : Event;
        private delegate void EventDelegate(Event e);

        private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
        private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();


        public void AddListener<T>(EventDelegate<T> del) where T : Event
        {
            if (delegateLookup.ContainsKey(del))
                return;

            EventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
                delegates[typeof(T)] = tempDel += internalDelegate;
            else
                delegates[typeof(T)] = internalDelegate;
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : Event
        {
            EventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                EventDelegate tempDel;
                if (delegates.TryGetValue(typeof(T), out tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                        delegates.Remove(typeof(T));
                    else
                        delegates[typeof(T)] = tempDel;
                }

                delegateLookup.Remove(del);
            }
        }

        public int DelegateLookupCount { get { return delegateLookup.Count; } }

        public void Raise(Event e)
        {
            EventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
                del.Invoke(e);
        }

    }
    
}
