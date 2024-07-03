using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Dusts;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Tiles
{
    public class ITDGlobalTile : GlobalTile
    {
        private readonly Asset<Texture2D> bluetopsglowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/BlueshroomGroves/BlueshroomTops_Glow");
        public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
        {
            if (type == TileID.Trees)
            {
                // this is a blueshroom tree
                if (Helpers.TileType(i, j + 1, ModContent.TileType<Bluegrass>()) && Helpers.TileType(i, j - 1, TileID.Trees))
                {
                    var pos = Helpers.GetTreeTopPosition(i, j);
                    if (pos != null)
                    {
                        Point tilePos = (Point)pos;
                        if (Main.rand.NextBool(20))
                        Dust.NewDust(tilePos.ToWorldCoordinates() - new Vector2(25, 50), 50, 62, ModContent.DustType<BlueshroomSporesDust>());
                    }
                }
            }
        }
    }
}