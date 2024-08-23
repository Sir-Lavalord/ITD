﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
    public class CatacombLampTile : ITDLamp
    {
        public override Texture2D FlameTexture => ModContent.Request<Texture2D>("ITD/Content/Tiles/Furniture/Catacombs/CatacombLampTile_Flame").Value;
        private static readonly Vector3 blueLight = new Vector3(0.47f, 0.84f, 1f);
        private static readonly Vector3 greenLight = new Vector3(0.94f, 0.76f, 0.59f);
        private static readonly Vector3 pinkLight = new Vector3(0.96f, 0.63f, 0.36f);
        public override void SetStaticLampDefaults()
        {
            DustType = DustID.Shadowflame;
            MapColor = new Color(3, 39, 105);
            LightColor = [blueLight, greenLight, pinkLight];
            EmitDust = [DustID.BlueTorch, DustID.GreenTorch, DustID.PinkTorch];
            AlternateDirection = true;
        }
    }
}