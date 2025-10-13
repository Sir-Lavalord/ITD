using System;

namespace ITD.Utilities.EntityAnim;

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
        if (IsFinished)
            return;
        playFrames++;
        if (Progress >= 1f)
        {
            IsFinished = true;
        }
        T interpolatedValue = MiscHelpers.LerpAny(_startValue, _endValue(), _easingFunc(Progress));
        _setter(interpolatedValue);
    }
}
