using System.Collections.Generic;
using UnityEngine;

namespace TaskSystem
{
    /// <summary>
    /// Drives every <see cref="Task"/> created while a manager instance exists. Tasks self-register on
    /// construction (see <see cref="Task"/>), so nested container children are updated directly by this
    /// manager - containers only decide when to Run() a child and react to its Done callback.
    ///
    /// Auto-creates itself on first use, so dropping this script in a scene is optional but recommended
    /// for visibility in the hierarchy of a demo/portfolio project.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TaskManager : MonoBehaviour
    {
        private static TaskManager _instance;
        private readonly List<Task> _tasks = new List<Task>();
        private bool _paused;

        public static TaskManager Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var go = new GameObject(nameof(TaskManager));
                _instance = go.AddComponent<TaskManager>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        public static void Register(Task task) => Instance._tasks.Add(task);

        public void SetPaused(bool paused) => _paused = paused;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            if (_paused || _tasks.Count == 0) return;

            // Snapshot so tasks that add new tasks mid-frame (e.g. containers wiring children)
            // don't mutate the list we're currently iterating.
            var snapshot = _tasks.ToArray();
            List<Task> completed = null;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var task = snapshot[i];
                if (task == null) continue;

                if (task.Completed || task.Update(Time.deltaTime))
                {
                    (completed ??= new List<Task>()).Add(task);
                }
            }

            if (completed == null) return;

            for (int i = 0; i < completed.Count; i++)
            {
                var task = completed[i];
                task.Complete();
                task.SetParent(null);
                _tasks.Remove(task);
            }
        }
    }
}
