using System;
using System.Collections.Generic;
using System.Text;
using ITD.Utilities.ObserverPatterns;
using static ITD.Utilities.StateMachines.LayeredStateMachine<T>;

///Should note that I've included 2 types of state machine. Use the one that fits the use case. You would be surprised by how often they could be used in places. But each one will have extra overhead.
namespace ITD.Utilities.StateMachines
{
    /// <summary>
    /// This is the common type that most people know. Here we use event dispatchers to allow for function hooking for when the state changes. 
    /// We seperately allow the use of SwitchingState to be inherited and added new functionality.
    /// </summary>
    public class SwitchingStateMachine
    {
        private List<SwitchingState> StateList = new List<SwitchingState>();
        public SwitchingState CurrentActiveState { get; private set; }

        public void AddState(SwitchingState state)
        {
            if (state == null || StateList.Contains(state)) return;
            state.SetOwningStateMachine(this);
            StateList.Add(state);
        }

        public void RemoveState(SwitchingState state)
        {
            if (state == null) return;
            if (CurrentActiveState == state)
            {
                state.OnStateExit?.Invoke();
                CurrentActiveState = null;
            }
            StateList.Remove(state);
        }

        public void SwitchState(SwitchingState newState)
        {
            if (newState == null || !StateList.Contains(newState)) return;

            CurrentActiveState?.OnStateExit?.Invoke();
            CurrentActiveState = newState;
            CurrentActiveState.OnStateEnter?.Invoke();
        }

        public void Reset()
        {
            CurrentActiveState?.OnStateExit?.Invoke();
            CurrentActiveState = null;
        }

        public IReadOnlyList<SwitchingState> GetAllStates() => StateList.AsReadOnly();

        public class SwitchingState
        {
            public SwitchingState(SwitchingStateMachine machine)
            {
                OwningStateMachine = machine;
            }

            public SwitchingStateMachine OwningStateMachine { get; private set; }

            public ITDListenerEvent OnStateEnter = new ITDListenerEvent();
            public ITDListenerEvent OnStateExit = new ITDListenerEvent();
        }
    }

    /// <summary>
    /// It should be noted that when you use a value like int in place of the T in the template, you cannot use LayeredStates that inherites a T that is of a float. 
    /// The function of LayeredStateMachine uses co-variance. Each State type needs to be seperated stated and used as an instance. In the event that you dont feel like pre stating the type, you can use interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LayeredStateMachine<T>
    {
        private List<LayeredState<T>> StateList = new List<LayeredState<T>>();

        public void AddState(LayeredState<T> state)
        {
            if (state == null || StateList.Contains(state)) return;
            StateList.Add(state);
        }

        public void RemoveState(LayeredState<T> state)
        {
            StateList.Remove(state);
        }

        public void ClearStates()
        {
            StateList.Clear();
        }

        public IReadOnlyList<LayeredState<T>> GetAllStates() => StateList.AsReadOnly();

        public T GetCompletedPass(T InitialVariable)
        {
            T result = InitialVariable;
            foreach (var state in StateList)
            {
                result = state.LayerPass(result);
            }
            return result;
        }

        public class LayeredState<T>
        {
            /// Note that an abstract can be used here, but that would require creating checks on implimentation point.
            protected internal virtual T LayerPass(T LastLayerValue)
            {
                return LastLayerValue; ///Default behavior
            }
        }
    }
}

namespace ITD.Utilities.StateMachines
{
    namespace SwitchingStates
    {
        public interface IUpdatedFunction
        {
            void Update();
        }
        /// <summary>
        /// This is the version of the switching state we will most commonly use. By hooking the state to the correct function in the code, we can entirely reference only the interface for behavior, saving resources.
        /// </summary>
        public class SwitchingStateUpdating : SwitchingStateMachine.SwitchingState, IUpdatedFunction
        {
            public ITDListenerEvent OnStateUpdate = new ITDListenerEvent();
            public void Update()
            {
                OnStateUpdate.Invoke();
            }
        }
    }
    namespace LayeredStates
    {
        public class LayeredStateAddition : LayeredState<float>
        {
            public float Addition;
            protected override float LayerPass(float LastLayerValue)
            {
                return LastLayerValue + Addition;
            }
        }
    }
}
