namespace TaskSystem
{
    /// <summary>Completes after <c>duration</c> seconds have elapsed (scaled by <see cref="TaskState"/>).</summary>
    public class DelayTask : Task
    {
        private readonly float _duration;

        public DelayTask(float duration, bool playOnReplay = true, bool skipableTask = false, bool autoSkip = false)
            : base(false, playOnReplay, skipableTask)
        {
            _duration = duration;
            if (autoSkip) OnSkip(Complete);
        }

        public override bool Execute() => RunningTime > _duration;
    }
}
