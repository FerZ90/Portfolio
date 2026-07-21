using DG.Tweening;
using TaskSystem;
using UnityEngine;
using UnityEngine.UI;

namespace TaskSystem.Demo
{
    /// <summary>
    /// Portfolio demo scene: shows the Task System (Sequential/Concurrent/Delay + DOTween-backed tasks)
    /// driving a UI menu open/close animation. Purely a UI/architecture showcase, not VR-specific.
    ///
    /// Open(): dimmer fades in, panel slides up into place, and buttons pop in one after another
    /// (each is its own SequentialTask of Delay -> Scale, all running inside one ConcurrentTask so the
    /// whole transition reports back through a single Done callback).
    /// </summary>
    public class MenuAnimationDemo : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private RectTransform menuPanel;
        [SerializeField] private Vector2 onscreenPos = Vector2.zero;
        [SerializeField] private Vector2 offscreenPos = new Vector2(0f, -800f);
        [SerializeField] private float panelMoveDuration = 0.45f;

        [Header("Dimmer")]
        [SerializeField] private CanvasGroup dimmerGroup;
        [SerializeField] private float dimmerFadeDuration = 0.3f;

        [Header("Buttons")]
        [SerializeField] private RectTransform[] buttonTransforms;
        [SerializeField] private float buttonScaleDuration = 0.35f;
        [SerializeField] private float buttonStagger = 0.08f;

        [Header("Triggers")]
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        private bool _isOpen;
        private Task _activeTransition;

        private void Awake()
        {
            SetHiddenStateImmediate();

            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void SetHiddenStateImmediate()
        {
            if (menuPanel != null) menuPanel.anchoredPosition = offscreenPos;

            if (dimmerGroup != null)
            {
                dimmerGroup.alpha = 0f;
                dimmerGroup.blocksRaycasts = false;
            }

            foreach (var rectTransform in buttonTransforms)
            {
                if (rectTransform != null) rectTransform.localScale = Vector3.zero;
            }
        }

        public void Open()
        {
            if (_isOpen || IsTransitioning()) return;

            if (dimmerGroup != null) dimmerGroup.blocksRaycasts = true;

            var root = new ConcurrentTask(autoRun: true);
            root.Add(new SetOpacityTask(true, dimmerGroup, 1f, useFade: true, fadeDuration: dimmerFadeDuration));
            root.Add(new MoveTask(true, menuPanel, offscreenPos, onscreenPos, panelMoveDuration, Ease.OutBack));

            for (int i = 0; i < buttonTransforms.Length; i++)
            {
                var rectTransform = buttonTransforms[i];
                float delay = i * buttonStagger;

                var stagger = new SequentialTask(autoRun: true);
                stagger.Add(new DelayTask(delay));
                stagger.Add(new ScaleTask(true, rectTransform, 0f, 1f, buttonScaleDuration, Ease.OutBack));
                root.Add(stagger);
            }

            root.Done(() => _isOpen = true);
            _activeTransition = root;
        }

        public void Close()
        {
            if (!_isOpen || IsTransitioning()) return;

            var root = new ConcurrentTask(autoRun: true);
            root.Add(new SetOpacityTask(true, dimmerGroup, 0f, useFade: true, fadeDuration: dimmerFadeDuration));
            root.Add(new MoveTask(true, menuPanel, onscreenPos, offscreenPos, panelMoveDuration, Ease.InCubic));

            foreach (var rectTransform in buttonTransforms)
            {
                root.Add(new ScaleTask(true, rectTransform, 1f, 0f, buttonScaleDuration * 0.6f, Ease.InBack));
            }

            root.Done(() =>
            {
                _isOpen = false;
                if (dimmerGroup != null) dimmerGroup.blocksRaycasts = false;
            });
            _activeTransition = root;
        }

        private bool IsTransitioning() => _activeTransition != null && _activeTransition.IsRunning;
    }
}
