using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria;
using ITD.Content.Projectiles.Friendly.Melee;

namespace ITD.Utilities.EntityAnim
{
    // think roblox's tweening system but you can sequence them together.
    public class EntityAnim<T>(Keyframe<T>[] keyframes) where T : struct
    {
        /// <summary>
        /// Whether this animation has finished playing.
        /// </summary>
        public bool IsFinished = false;
        /// <summary>
        /// The Entity that will play this animation.
        /// </summary>
        public Entity Entity { get; set; }
        /// <summary>
        /// Keyframes in the animation. These will play in the order they're appended in
        /// </summary>
        public Keyframe<T>[] Keyframes { get; set; } = keyframes;
        /// <summary>
        /// The index of the current playing Keyframe in the animation.
        /// </summary>
        public int FrameIndex { get; set; } = 0;
        private T PreviousValue { get; set; } = default;
        /// <summary>
        /// Only use this overload if you need to add Keyframes with different Entity targets into a single animation for whatever reason. Otherwise use the other two overloads.
        /// If using this overload, create new Keyframes using <see cref="AnimHelpers.CreateFor{T}(Entity, Expression{Func{T}}, Func{T}, int, EasingFunctions.EasingFunc)"/>
        /// </summary>
        public EntityAnim<T> Append(params Keyframe<T>[] addKeyframes)
        {
            return new EntityAnim<T>([.. Keyframes, .. addKeyframes])
            {
                Entity = Entity,
            };
        }
        /// <summary>
        /// Returns a new <see cref="EntityAnim{T}"/> which adds a <see cref="Keyframe{T}"/> with the given parameters to the end of the Keyframes array.
        /// </summary>
        public EntityAnim<T> Append(Expression<Func<T>> propertyExpr, Func<T> endValue, int frames, EasingFunctions.EasingFunc easingFunc)
        {
            Keyframe<T> newKeyframe = AnimHelpers.CreateFor(Entity, propertyExpr, endValue, frames, easingFunc);
            return new EntityAnim<T>([.. Keyframes, newKeyframe])
            {
                Entity = Entity,
            };
        }
        /// <inheritdoc cref="Append(Expression{Func{T}}, Func{T}, int, EasingFunctions.EasingFunc)"/>>
        public EntityAnim<T> Append(Expression<Func<T>> propertyExpr, T endValue, int frames, EasingFunctions.EasingFunc easingFunc)
        {
            Keyframe<T> newKeyframe = AnimHelpers.CreateFor(Entity, propertyExpr, () => endValue, frames, easingFunc);
            return new EntityAnim<T>([.. Keyframes, newKeyframe])
            {
                Entity = Entity,
            };
        }
        private void ResetKeyframe(Keyframe<T> keyframe)
        {
            keyframe.IsFinished = false;
            keyframe.playFrames = 0;
            keyframe.SetStartValue(PreviousValue);
        }
        /// <summary>
        /// This method must be called every frame until IsFinished is true. ik it's kinda annoying but I don't want to make an AnimationManager class
        /// </summary>
        public void Play(bool loop)
        {
            if (IsFinished)
            {
                if (loop)
                {
                    JumpTo(0, 0);
                }
                else
                    return;
            }
            int maxIndex = Keyframes.Length;
            if (FrameIndex > maxIndex - 1)
            {
                IsFinished = true;
                return;
            }
            Keyframe<T> playKeyframe = Keyframes[FrameIndex];
            playKeyframe.SetStartValue(PreviousValue);
            playKeyframe.Play();
            if (playKeyframe.IsFinished)
            {
                FrameIndex++;
                PreviousValue = playKeyframe.getter();
            }
        }
        /// <summary>
        /// Jumps to a specific point in the animation
        /// </summary>
        /// <param name="keyframe"></param>
        /// <param name="playFrame"></param>
        public void JumpTo(int keyframe, int playFrame)
        {
            IsFinished = false;
            FrameIndex = keyframe;

            for (int i = keyframe; i < Keyframes.Length; i++)
            {
                ResetKeyframe(Keyframes[i]);
            }

            if (Keyframes.Length > 0)
                Keyframes[keyframe].playFrames = playFrame;
        }
    }
    public class Keyframe<T>(Func<T> getter, Action<T> setter, Func<T> endValue, EasingFunctions.EasingFunc easingFunc) where T : struct
    {
        public bool IsFinished = false;
        public int frames;
        /// <summary>
        /// The actual frame value that changes.
        /// </summary>
        public int playFrames = 0;
        public float Progress { get { return (float)playFrames / frames; } }
        public readonly Func<T> getter = getter;
        private readonly Action<T> _setter = value => setter(value);
        private  T _startValue = getter();
        private readonly Func<T> _endValue = endValue;
        private readonly EasingFunctions.EasingFunc _easingFunc = easingFunc;
        public void SetStartValue(T lastValue)
        {
            _startValue = lastValue;
        }
        public void Play()
        {
            playFrames++;
            if (Progress >= 1f)
                IsFinished = true;
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
            else
            {
                throw new InvalidOperationException("Unsupported keyframe type");
                // feel free to add your own stuff here but i don't think there's anything else important to add
            }
            _setter(interpolatedValue);
        }
    }
    public static class AnimHelpers
    {
        /// <summary>
        /// Create a new EntityAnim for this Entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="globalPosition"></param>
        /// <returns></returns>
        public static EntityAnim<T> CreateAnim<T>(this Entity entity) where T : struct
        {
            EntityAnim<T> newAnim = new([])
            {
                Entity = entity
            };
            return newAnim;
        }
        public static Keyframe<T> CreateFor<T>(Entity target, Expression<Func<T>> propertyExpr, Func<T> endValue, int frames, EasingFunctions.EasingFunc easingFunc) where T : struct
        {
            var getter = propertyExpr.Compile();

            if (propertyExpr.Body is MemberExpression memberExpr)
            {
                if (memberExpr.Member is PropertyInfo property)
                {
                    var setter = (Action<T>)Delegate.CreateDelegate(
                        typeof(Action<T>),
                        target,
                        property.GetSetMethod() ?? throw new InvalidOperationException("Property has no setter")
                    );

                    return new Keyframe<T>(getter, setter, endValue, easingFunc)
                    {
                        frames = frames
                    };
                }
                else if (memberExpr.Member is FieldInfo field)
                {
                    Action<T> setter = value =>
                    {
                        field.SetValue(target, value);
                    };

                    return new Keyframe<T>(getter, setter, endValue, easingFunc)
                    {
                        frames = frames
                    };
                }
            }
            return null;
        }
    }
}
