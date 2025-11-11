using Daybreak.Common.Features.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ITD.Utilities.Tweening;

public static class Tweener
{
    private static readonly List<IKeyframe> _activeTweens = [];
    public static IKeyframe Tween<T>
        (Expression<Func<T>> exp, T endValue, int frames, Func<float, float> easingFunc = null, Action onFinish = null) where T : struct
    {
        var getter = exp.Compile();
        Action<T> setter = ((MemberExpression)exp.Body).Member switch
        {
            PropertyInfo property => property.GetSetMethod().CreateDelegate<Action<T>>(getter.Target),
            FieldInfo field => x => field.SetValue(getter.Target, x),
            _ => throw new InvalidOperationException()
        };
        return new Keyframe<T>(getter, setter, () => endValue, easingFunc ?? EasingFunctions.Linear, onFinish);
    }
    public static IKeyframe Tween(IKeyframe keyframe)
    {
        ArgumentNullException.ThrowIfNull(keyframe);
        _activeTweens.Add(keyframe);
        return keyframe;
    }
    public static void CancelTween(IKeyframe keyframe)
    {
        IKeyframe lookup = _activeTweens.FirstOrDefault(k => keyframe.Equals(keyframe), null);
        if (lookup != null)
        {
            keyframe.OnFinish();
            _activeTweens.Remove(keyframe);
        }
    }
    [SubscribesTo<ModSystemHooks.UpdateUI>]
    public static void UpdateUI(ModSystemHooks.UpdateUI.Original orig, ModSystem self, GameTime gt)
    {
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            IKeyframe t = _activeTweens[i];
            t.Update();
            if (t.IsFinished)
            {
                t.OnFinish();
                _activeTweens.RemoveAt(i);
            }
        }
    }
}
