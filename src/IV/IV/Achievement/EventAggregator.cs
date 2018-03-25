using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IV.Achievement
{
    public class EventAggregator : IEventAggregator
    {
        #region Singleton

        private static EventAggregator _eventAggregator;

        public static EventAggregator Instance
        {
            get { return _eventAggregator ?? (_eventAggregator = new EventAggregator()); }
        }

        private EventAggregator()
        {

        }

        #endregion

        private readonly Dictionary<Type, List<WeakReference>> _eventSubscribersList =
            new Dictionary<Type, List<WeakReference>>();

        private readonly object _lock = new object();

        public void Subscribe(object subscriber)
        {
            lock (_lock)
            {
                var subscriberTypes =
                    subscriber.GetType().GetInterfaces().Where(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISubscriber<>));

                var weakRefrence = new WeakReference(subscriber);
                foreach (var subscriberType in subscriberTypes)
                {
                    var subscribers = GetSubscribers(subscriberType);
                    subscribers.Add(weakRefrence);

                }
            }
        }

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            var subscriberType = typeof (ISubscriber<>).MakeGenericType(typeof (TEvent));
            var subscribers = GetSubscribers(subscriberType);
            var subscribersToRemove = new List<WeakReference>();

            foreach (var weakSubscriber in subscribers)
            {
                if(weakSubscriber.IsAlive)
                {
                    var subscriber = (ISubscriber<TEvent>) weakSubscriber.Target;

                    //if deferent thead
                    var sysContext = SynchronizationContext.Current ?? new SynchronizationContext();

                    sysContext.Post(s => subscriber.OnEvent(eventToPublish), null);
                }
                else
                {
                    subscribersToRemove.Add(weakSubscriber);
                }
            }

            if(subscribersToRemove.Any())
            {
                lock (_lock)
                {
                    subscribersToRemove.ForEach(s => subscribers.Remove(s));
                }
            }
        }

        private List<WeakReference> GetSubscribers(Type subscriberType)
        {
            List<WeakReference> subscribers;
            lock (_lock)
            {
                var found = _eventSubscribersList.TryGetValue(subscriberType, out subscribers);
                if(!found)
                {
                    subscribers = new List<WeakReference>();
                    _eventSubscribersList.Add(subscriberType, subscribers);
                }
            }
            return subscribers;
        }
    }
}