using System.Collections.Generic;

namespace TaskSystem
{
    /// <summary>
    /// Base class for tasks that own a list of child tasks (see <see cref="SequentialTask"/> and
    /// <see cref="ConcurrentTask"/>). A container never calls Update on its children - the
    /// <see cref="TaskManager"/> does that directly, since every child self-registered on construction.
    /// The container only calls Run() on children and reacts to their Done callback.
    /// </summary>
    public class TaskContainer : Task
    {
        private readonly List<Task> _children = new List<Task>();
        private bool _overrideChildrenIgnoreTimeScale = true;

        protected int ChildCount => _children.Count;

        public TaskContainer(bool autoRun = false, bool playOnReplay = true, bool skipableTask = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
        }

        public override void Add(Task child)
        {
            if (child == null) return;

            if (_overrideChildrenIgnoreTimeScale) child.IgnoreTimeScale = IgnoreTimeScale;
            child.SetParent(this);
            _children.Add(child);
        }

        public void Add(IEnumerable<Task> children)
        {
            if (children == null) return;
            foreach (var child in children) Add(child);
        }

        public bool HasChild(Task child) => _children.Contains(child);

        protected Task GetChildAt(int index) => _children[index];

        public override void Remove(Task child) => _children.Remove(child);

        public void SetIgnoreTimeScale(bool ignore)
        {
            IgnoreTimeScale = ignore;
            if (!_overrideChildrenIgnoreTimeScale) return;
            foreach (var child in _children) child.IgnoreTimeScale = ignore;
        }

        public void SetOverrideChildrenIgnoreTimeScale(bool value) => _overrideChildrenIgnoreTimeScale = value;

        public override void Stop()
        {
            foreach (var child in _children) child.Stop();
            base.Stop();
        }

        public override void Complete()
        {
            foreach (var child in _children) child.Complete();
            base.Complete();
        }

        public override void Skip(bool executeDoneAfterSkip = false)
        {
            foreach (var child in _children) child.Skip(executeDoneAfterSkip);
            base.Skip(executeDoneAfterSkip);
        }
    }
}
