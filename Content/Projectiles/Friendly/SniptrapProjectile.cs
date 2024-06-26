using ITD.Content.Items;
using ITD.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles
{
    public class SniptrapProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapProperties()
        {
            shootRange = 16f * 8f;
            retractAccel = 1.5f;
            extraFlexibility = 16f * 2f;
            framesBetweenHits = 16;
            minDamage = 1;
            maxDamage = 10;
            fullPowerHitsAmount = 10;
            warningFrames = 60;
            chompDust = DustID.Iron;
            toChainTexture = "ITD/Content/Projectiles/Friendly/SniptrapChain";
            snaptrapMetal = new SoundStyle("ITD/Content/Sounds/SniptrapClose", SoundType.Sound);
        }
    }
}