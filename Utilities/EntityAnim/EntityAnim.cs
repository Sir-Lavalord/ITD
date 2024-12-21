using System;
using System.Linq.Expressions;
using System.Reflection;
using Terraria;

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
        public EntityAnim<T> Append(Expression<Func<T>> propertyExpr, Func<T> endValue, int frames, Func<float, float> easingFunc)
        {
            Keyframe<T> newKeyframe = AnimHelpers.CreateFor(Entity, propertyExpr, endValue, frames, easingFunc);
            return new EntityAnim<T>([.. Keyframes, newKeyframe])
            {
                Entity = Entity,
            };
        }
        /// <inheritdoc cref="Append(Expression{Func{T}}, Func{T}, int, Func{T, TResult})"/>>
        public EntityAnim<T> Append(Expression<Func<T>> propertyExpr, T endValue, int frames, Func<float, float> easingFunc)
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
            playKeyframe.Update();
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
        public static Keyframe<T> CreateFor<T>(Entity target, Expression<Func<T>> propertyExpr, Func<T> endValue, int frames, Func<float, float> easingFunc) where T : struct
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
                    void Setter(T value)
                    {
                        field.SetValue(target, value);
                    }

                    return new Keyframe<T>(getter, Setter, endValue, easingFunc)
                    {
                        frames = frames
                    };
                }
            }
            return null;
        }
    }
}
