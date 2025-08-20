using System;
using ITD.Content.NPCs.Bosses;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Hostile.CosJel
{

    public class CosmicSetHandWarn : ModProjectile
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
            NPC Hand = Main.npc[(int)NPCWhoAmI];
            Projectile.Center = Hand.Center - new Vector2(0, 12);

            if (!LockIn)
            {

                Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(Hand.DirectionTo(Main.player[Hand.target].Center + Main.player[Hand.target].velocity * 20).ToRotation(), .2f).ToRotationVector2();
                Projectile.rotation = Projectile.velocity.ToRotation() - (float)Math.PI / 2;
            }

            if (--Timer <= 0)
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
            default(CosmicTelegraphVertex).Draw(Projectile.Center - Main.screenPosition, 
                new Vector2(Projectile.velocity.Length() * 3000, 80 * Projectile.scale), Projectile.rotation + MathHelper.PiOver2);

            return false;
        }

        public override void OnKill(int timeLeft)
        {

        }
    }
}