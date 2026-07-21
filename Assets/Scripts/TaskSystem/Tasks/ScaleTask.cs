using DG.Tweening;
using UnityEngine;

namespace TaskSystem
{
    /// <summary>
    /// Scales a Transform between two uniform scales using DOTween. Not present in the original Cocos
    /// reference - added for UI "pop-in" reveals (buttons, icons) inside a Sequential/Concurrent graph.
    /// </summary>
    public class ScaleTask : Task
    {
        private readonly Transform _transform;
        private readonly float _from;
        private readonly float _to;
        private readonly float _duration;
        private readonly Ease _ease;

        private Tween _tween;
        private bool _finished;

        public ScaleTask(bool autoRun, Transform transform, float from, float to, float duration,
            Ease ease = Ease.OutBack, bool playOnReplay = true, bool skipableTask = false, bool autoSkip = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
            _transform = transform;
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
            _transform.localScale = Vector3.one * _from;
            _tween = _transform
                .DOScale(_to, _duration)
                .SetEase(_ease)
                .OnComplete(() => _finished = true);

            _tween.timeScale = TaskState.GetTimeScale();
        }

        private void ApplyEndState()
        {
            _tween?.Kill();
            if (_transform != null) _transform.localScale = Vector3.one * _to;
            _finished = true;
        }
    }
}
