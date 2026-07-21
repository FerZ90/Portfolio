using DG.Tweening;
using UnityEngine;

namespace TaskSystem
{
    /// <summary>
    /// Moves a RectTransform's anchored position over time using DOTween. Wrapping DOTween in a Task
    /// lets UI motion compose with Sequential/Concurrent/Delay tasks like any other step in an
    /// animation graph. The tween only starts lazily on the first <see cref="Execute"/> call, so it
    /// works correctly whether this task is auto-run or started later by a container's Run().
    /// </summary>
    public class MoveTask : Task
    {
        private readonly RectTransform _rectTransform;
        private readonly Vector2 _from;
        private readonly Vector2 _to;
        private readonly float _duration;
        private readonly Ease _ease;

        private Tween _tween;
        private bool _finished;

        public MoveTask(bool autoRun, RectTransform rectTransform, Vector2 from, Vector2 to, float duration,
            Ease ease = Ease.OutCubic, bool playOnReplay = true, bool skipableTask = false, bool autoSkip = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
            _rectTransform = rectTransform;
            _from = from;
            _to = to;
            _duration = duration;
            _ease = ease;

            if (autoSkip) OnSkip(ApplyEndState);
        }

        public override bool Execute()
        {
            if (_tween == null) StartTween();
            return _finished;
        }

        private void StartTween()
        {
            _rectTransform.anchoredPosition = _from;
            _tween = _rectTransform
                .DOAnchorPos(_to, _duration)
                .SetEase(_ease)
                .OnComplete(() => _finished = true);

            // Respect TaskState (instant/skip-all mode reuses the same DOTween timeScale trick).
            _tween.timeScale = TaskState.GetTimeScale();
        }

        private void ApplyEndState()
        {
            _tween?.Kill();
            if (_rectTransform != null) _rectTransform.anchoredPosition = _to;
            _finished = true;
        }
    }
}
