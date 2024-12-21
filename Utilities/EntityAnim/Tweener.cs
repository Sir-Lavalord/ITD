using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ITD.Utilities.EntityAnim
{
    public class Tweener : ModSystem
    {
        private static readonly List<IKeyframe> _activeTweens = [];
        public override void Unload()
        {
            _activeTweens.Clear();
        }
        public static void Tween(IKeyframe keyframe)
        {
            ArgumentNullException.ThrowIfNull(keyframe);
            _activeTweens.Add(keyframe);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                IKeyframe t = _activeTweens[i];
                t.Update();
                if (t.IsFinished)
                    _activeTweens.RemoveAt(i);
            }
        }
    }
}
