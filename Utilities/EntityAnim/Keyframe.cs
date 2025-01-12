using System;

namespace ITD.Utilities.EntityAnim
{
    public interface IKeyframe
    {
        bool IsFinished { get; }
        void Update();
        void OnFinish();
    }
    public class Keyframe<T>(Func<T> getter, Action<T> setter, Func<T> endValue, Func<float, float> easingFunc, Action onFinish = null) : IKeyframe where T : struct
    {
        public bool IsFinished = false;
        public int frames;
        /// <summary>
        /// The actual frame value that changes.
        /// </summary>
        public int playFrames = 0;
        public float Progress { get { return (float)playFrames / frames; } }

        bool IKeyframe.IsFinished => IsFinished;

        public readonly Func<T> getter = getter;
        private readonly Action<T> _setter = value => setter(value);
        private T _startValue = getter();
        private readonly Func<T> _endValue = endValue;
        private readonly Func<float, float> _easingFunc = easingFunc;
        private readonly Action _onFinish = onFinish;
        public void SetStartValue(T lastValue)
        {
            _startValue = lastValue;
        }
        void IKeyframe.Update() => Update();
        void IKeyframe.OnFinish() => _onFinish?.Invoke();
        public void Update()
        {
            playFrames++;
            if (Progress >= 1f)
            {
                IsFinished = true;
                return;
            }
            T interpolatedValue = default;
            if (typeof(T) == typeof(float))
            {
                float start = (float)(object)_startValue;
                float end = (float)(object)_endValue();
                interpolatedValue = (T)(object)MathHelper.Lerp(start, end, _easingFunc(Progress));
            }
            else if (typeof(T) == typeof(Vector2))
            {
                Vector2 start = (Vector2)(object)_startValue;
                Vector2 end = (Vector2)(object)_endValue();
                interpolatedValue = (T)(object)Vector2.Lerp(start, end, _easingFunc(Progress));
            }
            else if (typeof(T) == typeof(Vector3))
            {
                Vector3 start = (Vector3)(object)_startValue;
                Vector3 end = (Vector3)(object)_endValue();
                interpolatedValue = (T)(object)Vector3.Lerp(start, end, _easingFunc(Progress));
            }
            else if (typeof(T) == typeof(Color))
            {
                Color start = (Color)(object)_startValue;
                Color end = (Color)(object)_endValue();
                interpolatedValue = (T)(object)Color.Lerp(start, end, _easingFunc(Progress));
            }
            else if (typeof(T) == typeof(Rectangle))
            {
                Rectangle start = (Rectangle)(object)_startValue;
                Rectangle end = (Rectangle)(object)_endValue();
                int lerpedX = (int)MathHelper.Lerp(start.X, end.X, _easingFunc(Progress));
                int lerpedY = (int)MathHelper.Lerp(start.Y, end.Y, _easingFunc(Progress));
                int lerpedW = (int)MathHelper.Lerp(start.Width, end.Width, _easingFunc(Progress));
                int lerpedH = (int)MathHelper.Lerp(start.Height, end.Height, _easingFunc(Progress));
                interpolatedValue = (T)(object)new Rectangle(lerpedX, lerpedY, lerpedW, lerpedH);
            }
            else
            {
                throw new InvalidOperationException("Unsupported keyframe type");
                // feel free to add your own stuff here but i don't think there's anything else important to add
            }
            _setter(interpolatedValue);
        }
    }
}
