using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Particles;
using System.Linq;
using ITD.Particles.Misc;
using Terraria.Audio;

namespace ITD.Content.Tiles.DeepDesert
{
    public class ReinforcedPegmatiteBricks : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            MinPick = 500;
            HitSound = SoundID.Tink;
            DustType = DustID.Sandstorm;

            AddMapEntry(new Color(199, 108, 91));
        }
        public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        {
            WorldGen.TileMergeAttempt(-2, ModContent.TileType<PegmatiteTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Point p = new(i, j);
            // here we check if the particle hasn't been mined (fail), and if there isn't already a particle that's linked to this tile
            if (fail && !ParticleSystem.Instance.particles.Any(prt => prt.tag is Point pnt && pnt == p))
            {
                SoundEngine.PlaySound(SoundID.Item15, p.ToWorldCoordinates());
                ITDParticle part = ParticleSystem.NewParticle<ProtectedTileParticle>(p.ToWorldCoordinates(), Vector2.Zero, 0f);
                part.tag = p;
            }
        }
    }
}
