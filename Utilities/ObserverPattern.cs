using System;
using System.Collections.Generic;
using System.Text;

namespace ITD.Utilities.ObserverPatterns
{
    public interface I_ITDMessageInterface
    {
        void RecievingMessage(string message);
    }

    public class ITDMessageInterfaceGroup
    {
        private readonly List<I_ITDMessageInterface> _observers = new List<I_ITDMessageInterface>();

        public void AddListener(I_ITDMessageInterface i_ITDMessageInterface)
        {
            _observers.Add(i_ITDMessageInterface);
        }

        public void RemoveListener(I_ITDMessageInterface i_ITDMessageInterface)
        {
            _observers.Remove(i_ITDMessageInterface);
        }

        public void SendMessage(string message)
        {
            foreach (I_ITDMessageInterface i_ in _observers)
            {
                i_.RecievingMessage(message); 
            }
        }
    }

    public class ITDListenerEvent<T>
    {
        private event Action<T> listeners;

        public void AddListener(Action<T> listener)
        {
            listeners += listener;
        }

        public void RemoveListener(Action<T> listener)
        {
            listeners -= listener;
        }

        public void Invoke(T value)
        {
            listeners?.Invoke(value);
        }

        public void RemoveAllListeners()
        {
            listeners = null;
        }
    }
}

/// <summary>
/// Below are mostly example code for its usage.
/// </summary>
private class Test
{
    ITD.Utilities.ObserverPatterns.ITDListenerEvent<int> TestEvent = new ITD.Utilities.ObserverPatterns.ITDListenerEvent<int>();
    ITD.Utilities.ObserverPatterns.ITDMessageInterfaceGroup TestMessageGroup = new ITD.Utilities.ObserverPatterns.ITDMessageInterfaceGroup();

    public void TestFunction(int i)
    {

    }

    public void EventFunction()
    {
        TestEvent.AddListener(TestFunction);
        TestEvent.RemoveListener(TestFunction);
    }
}