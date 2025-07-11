using Terraria.Audio;
using Terraria.GameContent;

using ITD.Utilities;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class MoulderBoulderProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public ref float BounceCount => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 5;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            if (Projectile.ai[0]++ > 15f)
            {
                Projectile.velocity.Y += 0.2f;
            }
            if (BounceCount>= 16)
            {
                Projectile.Kill();
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.t_Slime, 0, 0, 150, Color.Black, 1f);
                dust.noGravity = false;
            }
            Projectile.rotation += 0.4f;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            float power = 6 * Utils.GetLerpValue(800f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.GetITDPlayer().BetterScreenshake(4, power, power, false);
            target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);
            base.OnHitPlayer(target, info);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float power = 6 * Utils.GetLerpValue(800f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.GetITDPlayer().BetterScreenshake(4, power, power, false);
            target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 1.5f;
            modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            //go crazy
            Projectile.netUpdate = true;
            BounceCount++;
            Player player = Main.player[Main.myPlayer];
            float power = 12 * Utils.GetLerpValue(800f, 0f, Projectile.Distance(Main.LocalPlayer.Center),true);
            player.GetITDPlayer().BetterScreenshake(6, power, power, false);
            for (int i = 0; i < 3; i++)
            {
                Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector2 randVelocity = oldVelocity * 0.5f * Main.rand.NextFloat();
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Stone, randVelocity.X, randVelocity.Y, 150, default(Color), 2f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = oldVelocity.X * -1f;
            }

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = oldVelocity.Y * -1f;
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, 0, 0, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 trailPos = Projectile.oldPos[k] - Main.screenPosition + (Projectile.Size * 0.5f) + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, trailPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, SpriteEffects.None, 0f);
            }
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.Draw(projectileTexture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}