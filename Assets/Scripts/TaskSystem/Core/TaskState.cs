namespace TaskSystem
{
    /// <summary>
    /// Global playback controls for the whole task system: time scale and "instant" (skip) mode.
    /// Any Task that isn't marked <c>IgnoreTimeScale</c> reads this every frame, so toggling
    /// <see cref="SetInstantMode"/> fast-forwards every running task uniformly (e.g. "skip intro").
    /// DOTween-backed tasks (see MoveTask/ScaleTask/SetOpacityTask) apply this same value to their
    /// tween's own timeScale, so the trick works identically whether a task moves by hand or via DOTween.
    /// </summary>
    public static class TaskState
    {
        /// <summary>Large enough multiplier to make any tween/timer resolve within a single frame.</summary>
        public const float Infinity = 100000f;

        private static float _timeScale = 1f;
        private static bool _instant;

        public static bool IsInstantMode() => _instant;
        public static void SetInstantMode() => _instant = true;
        public static void SetNormalMode() => _instant = false;

        public static void SetTimeScale(float value) => _timeScale = value;

        public static float GetTimeScale() => _instant ? Infinity : _timeScale;
    }
}
