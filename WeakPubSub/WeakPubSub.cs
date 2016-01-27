using System;
using System.Collections.Generic;
using System.Windows;

namespace WeakPubSub
{
    public interface IPublisher<TEventArgs> where TEventArgs : EventArgs
    {
        event EventHandler<TEventArgs> Send;
    }

    public interface ISubscriber<TEventArgs> where TEventArgs : EventArgs
    {
        void Handler(Object sender, TEventArgs eventArgs);
    }

    public static class PubSubManager
    {
        public static void Publish<TKey, TEventArgs>(TKey key, IPublisher<TEventArgs> publisher) where TEventArgs : EventArgs
        {
            PubSubManager<TKey, TEventArgs>.Publish(key, publisher);
        }
        public static void Subscribe<TKey, TEventArgs>(TKey key, ISubscriber<TEventArgs> subscriber) where TEventArgs : EventArgs
        {
            PubSubManager<TKey, TEventArgs>.Subscribe(key, subscriber);
        }
        public static void Unpublish<TEventArgs>(String key) where TEventArgs : EventArgs
        {
            PubSubManager<String, TEventArgs>.Unpublish(key);
        }
        public static void Unpublish<TEventArgs>(Type key) where TEventArgs : EventArgs
        {
            PubSubManager<Type, TEventArgs>.Unpublish(key);
        }
        public static void Unpublish<TKey, TEventArgs>(TKey key) where TEventArgs : EventArgs
        {
            PubSubManager<TKey, TEventArgs>.Unpublish(key);
        }
        public static void Unsubscribe<TKey, TEventArgs>(TKey key, ISubscriber<TEventArgs> subscriber) where TEventArgs : EventArgs
        {
            PubSubManager<TKey, TEventArgs>.Unsubscribe(key, subscriber);
        }
    }

    public sealed class PubSubManager<TKey, TEventArgs> : WeakEventManager where TEventArgs : EventArgs
    {
        private readonly Dictionary<TKey, IPublisher<TEventArgs>> _publishers;

        public PubSubManager()
        {
            _publishers = new Dictionary<TKey, IPublisher<TEventArgs>>();
        }

        private void AddPublisher(TKey key, IPublisher<TEventArgs> publisher)
        {
            using (base.WriteLock)
                _publishers[key] = publisher;
        }
        private void AddSubscriber(TKey key, ISubscriber<TEventArgs> subscriber)
        {
            IPublisher<TEventArgs> publisher;
            using (base.ReadLock)
                publisher = _publishers[key];

            CurrentManager.ProtectedAddHandler(publisher, (EventHandler<TEventArgs>)subscriber.Handler);
        }
        protected override WeakEventManager.ListenerList NewListenerList()
        {
            return new WeakEventManager.ListenerList<TEventArgs>();
        }
        private void OnEventHandler(Object sender, TEventArgs eventArgs)
        {
            using (base.ReadLock)
            {
                var listenerList = (WeakEventManager.ListenerList)base[sender];
                base.DeliverEventToList(sender, eventArgs, listenerList);
            }
        }
        public static void Publish(TKey key, IPublisher<TEventArgs> publisher)
        {
            CurrentManager.AddPublisher(key, publisher);
        }
        private void RemovePublisher(TKey key)
        {
            using (base.WriteLock)
            {
                IPublisher<TEventArgs> publisher = _publishers[key];
                _publishers.Remove(key);
                base.Remove(publisher);
                StopListening(publisher);
            }
        }

        private void RemoveSubscriber(TKey key, ISubscriber<TEventArgs> subscriber)
        {
            IPublisher<TEventArgs> publisher;
            using (base.ReadLock)
                publisher = _publishers[key];

            CurrentManager.ProtectedRemoveHandler(publisher, (EventHandler<TEventArgs>)subscriber.Handler);
        }
        protected override void StartListening(Object source)
        {
            var publisher = (IPublisher<TEventArgs>)source;
            publisher.Send += OnEventHandler;
        }
        protected override void StopListening(Object source)
        {
            var publisher = (IPublisher<TEventArgs>)source;
            publisher.Send -= OnEventHandler;
        }
        public static void Subscribe(TKey key, ISubscriber<TEventArgs> subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            CurrentManager.AddSubscriber(key, subscriber);

        }
        public static void Unpublish(TKey key)
        {
            CurrentManager.RemovePublisher(key);
        }
        public static void Unsubscribe(TKey key, ISubscriber<TEventArgs> subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            CurrentManager.RemoveSubscriber(key, subscriber);
        }

        private static PubSubManager<TKey, TEventArgs> CurrentManager
        {
            get
            {
                Type typeFromHandle = typeof(PubSubManager<TKey, TEventArgs>);
                var eventManager = (PubSubManager<TKey, TEventArgs>)WeakEventManager.GetCurrentManager(typeFromHandle);
                if (eventManager == null)
                {
                    eventManager = new PubSubManager<TKey, TEventArgs>();
                    WeakEventManager.SetCurrentManager(typeFromHandle, eventManager);
                }
                return eventManager;
            }
        }
    }
}
