using System;
using System.Collections.Generic;

namespace TaskSystem
{
    /// <summary>
    /// Base unit of work in the task system. A Task moves through NotStarted -> Running -> Completed.
    /// Concrete tasks override <see cref="Execute"/> to do their per-frame work and return true once finished.
    ///
    /// Every task self-registers with <see cref="TaskManager"/> on construction, mirroring the original
    /// architecture: the manager drives every task directly (including nested children), while containers
    /// (<see cref="TaskContainer"/>, <see cref="SequentialTask"/>, <see cref="ConcurrentTask"/>) only decide
    /// *when* a child starts (Run) and react to its <see cref="Done"/> callback - they never call
    /// child.Update themselves.
    /// </summary>
    public class Task
    {
        public TaskStates State { get; protected set; }
        public float RunningTime { get; protected set; }
        public float Dt { get; protected set; }
        public bool IgnoreTimeScale { get; set; }
        public bool PlayOnReplay { get; }
        public bool SkipableTask { get; }
        public Task Parent { get; private set; }

        public bool Completed => State == TaskStates.Completed;
        public bool NotStarted => State == TaskStates.NotStarted;
        public bool IsRunning => State == TaskStates.Running;

        private readonly List<Action> _doneCallbacks = new List<Action>();
        private readonly List<Action> _skipCallbacks = new List<Action>();
        private bool _stateChanged;

        public Task(bool autoRun = false, bool playOnReplay = true, bool skipableTask = false)
        {
            PlayOnReplay = playOnReplay;
            SkipableTask = skipableTask;
            State = autoRun ? TaskStates.Running : TaskStates.NotStarted;

            TaskManager.Register(this);
        }

        /// <summary>Per-frame work. Return true when the task is finished.</summary>
        public virtual bool Execute() => true;

        public virtual void Add(Task child) { }
        public virtual void Remove(Task child) { }

        protected void ChangeState(TaskStates newState)
        {
            _stateChanged = newState != State;
            State = newState;
        }

        public bool StateChanged()
        {
            bool changed = _stateChanged;
            _stateChanged = false;
            return changed;
        }

        public virtual Task Run()
        {
            if (State == TaskStates.NotStarted)
                ChangeState(TaskStates.Running);
            return this;
        }

        /// <summary>
        /// Called every frame by <see cref="TaskManager"/> (not a MonoBehaviour Update). Advances
        /// <see cref="RunningTime"/> using <see cref="TaskState"/>'s time scale and calls <see cref="Execute"/>.
        /// </summary>
        public virtual bool Update(float dt)
        {
            if (State != TaskStates.Running) return false;

            Dt = IgnoreTimeScale ? dt : dt * TaskState.GetTimeScale();
            RunningTime += Dt;
            return Execute();
        }

        public virtual void Complete()
        {
            if (State != TaskStates.Running) return;
            ChangeState(TaskStates.Completed);
            InvokeDone();
        }

        public virtual void Skip(bool executeDoneAfterSkip = false)
        {
            if (State == TaskStates.Completed) return;
            ChangeState(TaskStates.Completed);
            InvokeSkip();
            if (executeDoneAfterSkip) InvokeDone();
        }

        public Task Done(Action callback)
        {
            _doneCallbacks.Add(callback);
            return this;
        }

        public Task OnSkip(Action callback)
        {
            _skipCallbacks.Add(callback);
            return this;
        }

        protected void InvokeDone()
        {
            for (int i = 0; i < _doneCallbacks.Count; i++) _doneCallbacks[i]?.Invoke();
        }

        protected void InvokeSkip()
        {
            for (int i = 0; i < _skipCallbacks.Count; i++) _skipCallbacks[i]?.Invoke();
        }

        public Task LocalIgnoreTimeScale()
        {
            IgnoreTimeScale = true;
            return this;
        }

        public virtual void Stop()
        {
            ChangeState(TaskStates.Completed);
            _doneCallbacks.Clear();
            _skipCallbacks.Clear();
        }

        public void SetParent(Task parent) => Parent = parent;
        public Task GetParent() => Parent;
    }
}
