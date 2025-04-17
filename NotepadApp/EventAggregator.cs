using System;
using System.Collections.Generic;

namespace NotepadApp.Events
{
    public static class EventAggregator
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();

        // Subscribe to an event
        public static void Subscribe<TEvent>(Action<TEvent> action)
        {
            if (!_subscriptions.ContainsKey(typeof(TEvent)))
            {
                _subscriptions[typeof(TEvent)] = new List<Delegate>();
            }

            _subscriptions[typeof(TEvent)].Add(action);
        }

        // Unsubscribe from an event
        public static void Unsubscribe<TEvent>(Action<TEvent> action)
        {
            if (_subscriptions.ContainsKey(typeof(TEvent)))
            {
                _subscriptions[typeof(TEvent)].Remove(action);
            }
        }

        // Publish an event
        public static void Publish<TEvent>(TEvent eventData)
        {
            if (_subscriptions.ContainsKey(typeof(TEvent)))
            {
                foreach (var action in _subscriptions[typeof(TEvent)])
                {
                    (action as Action<TEvent>)?.Invoke(eventData);
                }
            }
        }
    }
}
