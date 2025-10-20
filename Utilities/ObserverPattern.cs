using System;
using System.Collections.Generic;

namespace ITD.Utilities;

public interface IITDMessageInterface
{
    void RecievingMessage(string message);
}

public class ITDMessageInterfaceGroup
{
    private readonly List<IITDMessageInterface> _observers = [];

    public void AddListener(IITDMessageInterface iITDMessageInterface)
    {
        _observers.Add(iITDMessageInterface);
    }

    public void RemoveListener(IITDMessageInterface iITDMessageInterface)
    {
        _observers.Remove(iITDMessageInterface);
    }

    public void SendMessage(string message)
    {
        foreach (IITDMessageInterface i_ in _observers)
        {
            i_.RecievingMessage(message);
        }
    }
}

public class ITDListenerEvent
{
    private event Action Listeners;
    public void AddListener(Action listener)
    {
        Listeners += listener;
    }

    public void RemoveListener(Action listener)
    {
        Listeners -= listener;
    }

    public void Invoke()
    {
        Listeners?.Invoke();
    }

    public void RemoveAllListeners()
    {
        Listeners = null;
    }
}

public class ITDListenerEvent<T>
{
    private event Action<T> Listeners;

    public void AddListener(Action<T> listener)
    {
        Listeners += listener;
    }

    public void RemoveListener(Action<T> listener)
    {
        Listeners -= listener;
    }

    public void Invoke(T value)
    {
        Listeners?.Invoke(value);
    }

    public void RemoveAllListeners()
    {
        Listeners = null;
    }
}

#if false
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
#endif