using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader;
using Terraria;
using ITD.Content.NPCs.Bosses;
using Terraria.ID;
using ITD.Content.Items.Accessories.Expert;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Hostile
{

    public class CosmicRayWarn : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;


        public override bool? CanDamage()
        {
            return false;
        }
        private bool LockIn => Projectile.ai[2] != 0;
        private ref float Timer => ref Projectile.ai[0];
        private ref float NPCWhoAmI => ref Projectile.ai[1];
        private float maxTime = 240;
      
        public override void OnSpawn(IEntitySource source)
        {
            maxTime = Timer;
        }
        public override void AI()
        {
            NPC CosJel = Main.npc[(int)NPCWhoAmI];
            Projectile.Center = CosJel.Center - new Vector2(0,12);

            if (!LockIn) 
            {

                Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(CosJel.DirectionTo(Main.player[CosJel.target].Center + Main.player[CosJel.target].velocity * 20).ToRotation(), .1f).ToRotationVector2();
                Projectile.rotation = Projectile.velocity.ToRotation() - (float)Math.PI / 2;
            }

            if(--Timer <= 0) 
            {
                if (!LockIn)
                {
                    Projectile.ai[2] = -1;
                    Timer = maxTime / 3;

                }
                else
                    Projectile.Kill();


            }




        }

        public override bool PreDraw(ref Color lightColor)
        {
            default(CosmicTelegraphVertex).Draw(Projectile.Center - Main.screenPosition, new Vector2(Projectile.velocity.Length() * CosmicRay.MAX_LASER_LENGTH, 128 * Projectile.scale), Projectile.rotation + MathHelper.PiOver2);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CosmicJellyfish CosJel = (CosmicJellyfish)Main.npc[(int)Projectile.ai[1]].ModNPC;
            if (CosJel.NPC.active && CosJel.NPC.type == ModContent.NPCType<CosmicJellyfish>())
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile ray = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY), ModContent.ProjectileType<CosmicRay>(), Projectile.damage, Projectile.knockBack, -1, Projectile.ai[1], Projectile.rotation);
                    if(CosJel.AttackID != 7) 
                    {
                        ray.localAI[0] = 1;//mog(locked in)
                        ray.localAI[1] = 1;//slop
                        ray.localAI[2] = 1;//water
                        ray.timeLeft = 800;
                    }
                }
            }
        }
    }
}