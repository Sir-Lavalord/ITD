using System;
using System.Collections.Generic;
using System.Linq;

namespace ITD.Utilities.EntityAnim
{
    public class Tweener : ModSystem
    {
        private static readonly List<IKeyframe> _activeTweens = [];
        public override void Unload()
        {
            _activeTweens.Clear();
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
            /*
            else
                Main.NewText("Error trying to cancel the given tween");
            */
        }
        public override void UpdateUI(GameTime gameTime)
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
}
