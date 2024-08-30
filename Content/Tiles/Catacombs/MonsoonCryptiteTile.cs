using ITD.Content.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.Tiles.Catacombs
{
    public class MonsoonCryptiteTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item85;
            DustType = DustID.WaterCandle;
            AddMapEntry(new Color(45, 45, 101));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(16) && !Main.gameInactive)
                Rain.NewRainForced(new Point(i, j).ToWorldCoordinates() + Vector2.UnitY * 16f, new Vector2(8f, 16f));
        }
    }
}
