using System;

namespace TaskSystem
{
    /// <summary>
    /// Runs <paramref name="updateCallback"/> every frame only while <c>conditionalRunCb</c> stays true.
    /// If the condition turns false before the task finishes on its own, it completes early and invokes
    /// <c>fallbackCb</c> instead - useful for "wait while X" gates that must bail out safely
    /// (e.g. cancel a UI task if its target was destroyed mid-animation).
    /// </summary>
    public class ConditionalTask : Task
    {
        private readonly Func<Task, float, bool> _updateCallback;
        private readonly Func<bool> _conditionalRunCb;
        private readonly Action<Task, float> _fallbackCb;

        public ConditionalTask(
            Func<Task, float, bool> updateCallback,
            Func<bool> conditionalRunCb,
            Action<Task, float> fallbackCb = null,
            bool autoRun = false,
            bool playOnReplay = true,
            bool skipableTask = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
            _updateCallback = updateCallback ?? throw new ArgumentNullException(nameof(updateCallback));
            _conditionalRunCb = conditionalRunCb ?? throw new ArgumentNullException(nameof(conditionalRunCb));
            _fallbackCb = fallbackCb;
        }

        public override bool Execute() => _updateCallback(this, Dt);

        public override bool Update(float dt)
        {
            if (!_conditionalRunCb())
            {
                _fallbackCb?.Invoke(this, dt);
                Complete();
                return false;
            }

            if (State != TaskStates.Running) return false;

            Dt = IgnoreTimeScale ? dt : dt * TaskState.GetTimeScale();
            RunningTime += Dt;
            return Execute();
        }
    }
}
