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

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class SniptrapProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapProperties()
        {
            ShootRange = 16f * 8f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            FramesBetweenHits = 16;
            MinDamage = 1;
            MaxDamage = 10;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.Iron;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/SniptrapChain";
            toSnaptrapMetal = "ITD/Content/Sounds/SniptrapClose";
        }
    }
}