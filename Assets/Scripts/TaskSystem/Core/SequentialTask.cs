using System.Collections.Generic;

namespace TaskSystem
{
    /// <summary>Runs its children one after another; completes once the last one is done.</summary>
    public class SequentialTask : TaskContainer
    {
        private int _nextTaskIndex = -1;
        private bool _allTasksCompleted;

        public SequentialTask(bool autoRun = false, bool playOnReplay = true, bool skipableTask = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
        }

        public override bool Execute()
        {
            if (_nextTaskIndex < 0) RunNext();
            return _allTasksCompleted;
        }

        private bool RunNext()
        {
            _nextTaskIndex++;
            if (_nextTaskIndex < ChildCount)
            {
                var current = GetChildAt(_nextTaskIndex);
                if (current.Completed) return RunNext();

                current.Done(() => RunNext());
                if (current.NotStarted) current.Run();
                return false;
            }

            _allTasksCompleted = true;
            return true;
        }

        public static SequentialTask Create(IEnumerable<Task> tasks, bool autoRun, bool playOnReplay = true, bool skipableTask = false)
        {
            var seq = new SequentialTask(autoRun, playOnReplay, skipableTask);
            if (tasks != null) seq.Add(tasks);
            return seq;
        }

        public static SequentialTask CreateWithDelay(IEnumerable<Task> tasks, bool autoRun, float delay, bool playOnReplay = false, bool skipableTask = false)
        {
            var seq = new SequentialTask(autoRun, playOnReplay, skipableTask);
            seq.Add(new DelayTask(delay));
            if (tasks != null) seq.Add(tasks);
            return seq;
        }
    }
}
