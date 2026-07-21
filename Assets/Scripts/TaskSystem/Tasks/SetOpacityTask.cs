using DG.Tweening;
using UnityEngine;

namespace TaskSystem
{
    /// <summary>
    /// Sets or fades a CanvasGroup's alpha. With <c>useFade</c> false it applies instantly (one frame);
    /// with it true, animates via DOTween's <c>DOFade</c>, composing with the rest of the task graph.
    /// </summary>
    public class SetOpacityTask : Task
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly float _opacity;
        private readonly bool _useFade;
        private readonly float _fadeDuration;
        private readonly Ease _ease;

        private Tween _tween;
        private bool _finished;

        public SetOpacityTask(bool autoRun, CanvasGroup canvasGroup, float opacity, bool useFade = false,
            float fadeDuration = 0f, Ease ease = Ease.Linear, bool playOnReplay = true, bool skipableTask = false,
            bool autoSkip = false)
            : base(autoRun, playOnReplay, skipableTask)
        {
            _canvasGroup = canvasGroup;
            _opacity = opacity;
            _useFade = useFade;
            _fadeDuration = fadeDuration;
            _ease = ease;

            if (autoSkip) OnSkip(ApplyEndState);
        }

        public override bool Execute()
        {
            if (!_useFade)
            {
                _canvasGroup.alpha = _opacity;
                return true;
            }

            if (_tween == null)
            {
                _tween = _canvasGroup
                    .DOFade(_opacity, _fadeDuration)
                    .SetEase(_ease)
                    .OnComplete(() => _finished = true);

                _tween.timeScale = TaskState.GetTimeScale();
            }

            return _finished;
        }

        private void ApplyEndState()
        {
            _tween?.Kill();
            if (_canvasGroup != null) _canvasGroup.alpha = _opacity;
            _finished = true;
        }
    }
}
