using System.Collections.Generic;

namespace TaskSystem
{
    /// <summary>Runs all its children in parallel; completes once every child is done.</summary>
    public class ConcurrentTask : TaskContainer
    {
        private bool _started;

        public ConcurrentTask(bool autoRun = false, bool playOnReplay = true, bool skipableTask = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
        }

        public override bool Execute()
        {
            if (!_started)
            {
                _started = true;
                RunAllChildren();
            }
            return AllChildrenCompleted();
        }

        private void RunAllChildren()
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child.NotStarted) child.Run();
            }
        }

        private bool AllChildrenCompleted()
        {
            for (int i = 0; i < ChildCount; i++)
            {
                if (!GetChildAt(i).Completed) return false;
            }
            return true;
        }

        public static ConcurrentTask Create(IEnumerable<Task> tasks, bool autoRun, bool playOnReplay = true, bool skipableTask = false)
        {
            var con = new ConcurrentTask(autoRun, playOnReplay, skipableTask);
            if (tasks != null) con.Add(tasks);
            return con;
        }
    }
}
