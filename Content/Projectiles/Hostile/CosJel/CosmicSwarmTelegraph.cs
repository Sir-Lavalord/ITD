using ITD.Content.NPCs.Bosses;
using System;
using System.IO;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicSwarmTelegraph : ModProjectile
    {
        public Player player => Main.player[Projectile.owner];
        public override string Texture => ITD.BlankTexture;
        private float Side => Projectile.ai[2];
        public bool hideRay;
        public bool LockIn;
        public readonly int maxLength = 3000;
        public override bool? CanDamage()
        {
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public Vector2 PlayerOffset = Vector2.Zero;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }
        public readonly float lerp = 0.1f;
        public Vector2 lockPos;
        public override void AI()
        {
            Player player = Main.player[(int)Projectile.ai[0]];
            Projectile.rotation = MathHelper.PiOver2 * Side;
            if (!LockIn)
            {
                switch (Side)
                {
                    case 0:
                        Projectile.Center = new Vector2(MathHelper.Lerp(Projectile.Center.X, player.Center.X, lerp), player.Center.Y - (float)(Main.screenHeight / 1.25f));
                        break;
                    case 1:
                        Projectile.Center = new Vector2(player.Center.X + (float)(Main.screenWidth / 1.25f), MathHelper.Lerp(Projectile.Center.Y, player.Center.Y, lerp));
                        break;
                    case 2:
                        Projectile.Center = new Vector2(MathHelper.Lerp(Projectile.Center.X, player.Center.X, lerp), player.Center.Y + (float)(Main.screenHeight / 1.25f));
                        break;
                    case 3:
                        Projectile.Center = new Vector2(player.Center.X - (float)(Main.screenWidth / 1.25f), MathHelper.Lerp(Projectile.Center.Y, player.Center.Y, lerp));
                        break;
                }

                {

                }
                ;
                if (Projectile.ai[1] <= 120)
                {
                    Projectile.ai[1]+=2;

                }
                else
                {
                    if (Projectile.localAI[0]++ >= 120)
                    {
                        lockPos = Projectile.Center;
                        LockIn = true;
                    }
                }
            }
            else
            {
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] % 10 == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Square(-50, 50);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-10, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2) * 2, ModContent.ProjectileType<CosmicSwarm>(), 20, 0, -1, player.whoAmI);
                    }
                }
                Projectile.Center = lockPos;
            }
        } 
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!LockIn)
            default(CosmicTelegraphVertex).Draw(Projectile.Center - Main.screenPosition, new Vector2(Projectile.velocity.Length() * 3000, Projectile.ai[1] * Projectile.scale), Projectile.rotation + MathHelper.PiOver2);

            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
    }
}
        