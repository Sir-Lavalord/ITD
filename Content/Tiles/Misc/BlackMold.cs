using ITD.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.Tiles.Misc
{
    public class BlackMold : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.Stone] = true;
            Main.tileMerge[TileID.Stone][Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

            HitSound = SoundID.NPCHit9;
            DustType = DustID.Stone;

            AddMapEntry(Color.Black);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ItemID.StoneBlock);
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Player player = Main.LocalPlayer;
            Vector2 worldPos = new Point(i, j).ToWorldCoordinates();
            if (Main.rand.NextBool(16))
            {
                Dust d = Dust.NewDustDirect(worldPos - Vector2.One * 8f, 16, 16, DustID.t_Slime, 0f, 0f, 0, Color.Black);
                d.noGravity = true;
            }
            if (!player.Exists() || !Collision.CanHit(worldPos, 1, 1, player.Center, 1, 1))
                return;
            float range = 96f;
            if (Vector2.DistanceSquared(worldPos, player.Center) < range * range)
                player.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 59);
        }
        public override void RandomUpdate(int i, int j)
        {
            Helpers.GrowLongMossForTile(i, j, ModContent.TileType<LongBlackMold>(), Type);
        }
    }
}
