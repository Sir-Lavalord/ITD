using ITD.Content.Items.Weapons.Summoner;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using ITD.Content.NPCs.Bosses;
using Terraria.ID;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicWarning : ModProjectile
    {
        public Player player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 250;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public Vector2 PlayerOffset = Vector2.Zero;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }
        int loop;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }
        public override void AI()
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                Player player = Main.player[CosJel.target];
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] % 10 == 0 && loop >= 8)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnPos = Projectile.Center + new Vector2(500,0) + Main.rand.NextVector2Square(-50, 50) * Vector2.UnitY * 1;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-10, 0) * 2, ModContent.ProjectileType<CosmicSwarm>(), 20, 0, -1, CosJel.whoAmI);
                    }
                }
                if (++Projectile.frameCounter >= 10)
                {
                    loop++;
                    Projectile.frameCounter = 0;
                    Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                }
                if (PlayerOffset == Vector2.Zero)
                {
                    PlayerOffset = player.Center - Projectile.Center;
                }
                if (loop >= 8)
                {
                    Projectile.alpha = 255;
                    PlayerOffset.Y = 0;
                }
                else
                {
                    Projectile.Center = player.Center - PlayerOffset;
                }

            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
    }
}
        