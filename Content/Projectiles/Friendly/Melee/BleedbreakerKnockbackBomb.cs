using ITD.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class BleedbreakerKnockbackBomb : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10000;
            Projectile.tileCollide = false;
            Projectile.light = 0.75f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        private int MainTarget
        {
            get { return (int)Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC npc = Main.npc[MainTarget];
            Projectile.direction = (int)Projectile.ai[1];
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (target == Main.npc[MainTarget])
            {
                return false;
            }
            else return default;
        }
        public bool hasOtherTarget;
        public override void AI()
        {
            Projectile.direction = (int)Projectile.ai[1];
            NPC npc = Main.npc[MainTarget];
            if (!npc.active || npc.life <= 0)
            {
                Projectile.Kill();
                return;
            }
            if (!hasOtherTarget)
            {
                if (Main.rand.NextBool(5))
                {
                    Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, -npc.velocity.RotatedByRandom(MathHelper.ToRadians(30)), Main.rand.Next(825,828));
                    gore.timeLeft = 60;
                    gore.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    gore.velocity = -npc.velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    Gore gore1 = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, -npc.velocity.RotatedByRandom(MathHelper.ToRadians(30)), Main.rand.Next(825, 828));
                    gore1.timeLeft = 60;
                    gore1.velocity = -npc.velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    gore1.scale = Main.rand.NextFloat(0.8f, 1.2f);

                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 0, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                    int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 0, default, 1f);
                    Main.dust[dust2].noGravity = false;
                    Main.dust[dust2].velocity *= 1f;
                }
                Projectile.Center = npc.Center;
                if (npc.velocity == Vector2.Zero)
                {
                    Projectile.Kill();
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasOtherTarget)
            {
                hasOtherTarget = true;
                NPC npc = Main.npc[MainTarget];
                for (int i = 0; i < 20; i++)
                {
                    int dust = Dust.NewDust(Projectile.position, 1, 1, DustID.RedTorch, 0, 0, 0, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = new Vector2(0, 15).RotatedByRandom(4f) * Main.rand.NextFloat(0.9f, 1.1f);
                }
                Projectile.Resize(160, 160);
                Projectile.timeLeft = 3;
                float power = 10 * Utils.GetLerpValue(1200f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Player player = Main.player[Projectile.owner];
                player.GetITDPlayer().BetterScreenshake(10, power, power, false);
                npc.velocity.X = - Projectile.direction * 2f;
                if (target.Gimmickable())
                {

                    target.velocity.Y = Main.rand.NextFloat(-2, -1);
                    target.velocity.X = Projectile.direction * 4f;
                }
                }
            }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            NPC npc = Main.npc[MainTarget];
            if (!npc.active || npc.life <= 0)
            {
                Projectile.Kill();
                return false;
            }
            Texture2D texture2D13 = TextureAssets.Npc[npc.type].Value;
            int vertSize = texture2D13.Height / Main.npcFrameCount[npc.type];
            Vector2 origin = new Vector2(texture2D13.Width / 2f, texture2D13.Height / 2f / Main.npcFrameCount[npc.type]);
            Rectangle frameRect = new Rectangle(0, npc.frame.Y, texture2D13.Width, vertSize);

            Color color26 = lightColor;
            color26 = npc.GetAlpha(color26);

            SpriteEffects effects = npc.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
                {
                    Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Vector2 value4 = npc.oldPos[i];
                    float num165 = npc.oldRot[i];
                    Main.EntitySpriteDraw(texture2D13, value4 + npc.Size / 2f - Main.screenPosition + new Vector2(0, npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(frameRect), color27, num165, origin, npc.scale, SpriteEffects.FlipVertically, 0);
                }
            
            return false;
        }
    }
}
