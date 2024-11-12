using ITD.Utilities.EntityAnim;
using ITD.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System;
using static ITD.Utilities.EntityAnim.EasingFunctions;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ITD.Content.Dusts;
using Terraria.ID;

namespace ITD.Content.Projectiles.Unused
{
    public class DespoticSuperMeleeProjOld : ModProjectile
    {
        public ref float Scale => ref Projectile.ai[0];
        public ref float FadeIn => ref Projectile.ai[1];
        public override string Texture => "ITD/Content/Items/Weapons/Melee/DespoticSuperMeleeSword";
        public const float VisualLength = 60f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            FadeIn = 0f;
        }
        public void SpawnDust()
        {
            Vector2 thisMag = Projectile.velocity.SafeNormalize(Vector2.Zero) * VisualLength * Projectile.scale;
            thisMag.Along(Main.player[Projectile.owner].MountedCenter, 3f, (Vector2 point) => Dust.NewDust(point, 1, 1, ModContent.DustType<DespoticDust>()));
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = Scale;
            Projectile.width = (int)(Projectile.width * Scale);
            Projectile.height = (int)(Projectile.height * Scale);
            SpawnDust();
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDust();
        }
        public EntityAnim<Vector2> anim = null;
        public EntityAnim<Vector2> GetAnim()
        {
            return Projectile.CreateAnim<Vector2>()
                .Append(() => Projectile.velocity, FirstFrameDirection, 30, InOutQuad)
                .Append(() => Projectile.velocity, SecondFrameDirection, 10, InQuad);
        }
        public float GetRot()
        {
            return Main.player[Projectile.owner].LookDirection().ToRotation();
        }
        private Vector2 FrameDirection(int dirMultiplier)
        {
            int dir = Math.Sign(Main.player[Projectile.owner].direction) * dirMultiplier;
            return (Vector2.UnitX * 32f).RotatedBy(dir).RotatedBy(GetRot());
        }
        public Func<Vector2> FirstFrameDirection => () => FrameDirection(-1);
        public Func<Vector2> SecondFrameDirection => () => FrameDirection(1);
        public override void AI()
        {
            if (FadeIn < 1f)
                FadeIn += 0.05f;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
            Projectile.rotation = Projectile.velocity.ToRotation();
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(Projectile.position + (Projectile.velocity * 2f), Projectile.width, Projectile.height, DustID.IceTorch);
                Dust.NewDust(Projectile.position + (Projectile.velocity * 2f), Projectile.width, Projectile.height, ModContent.DustType<DespoticDust>());
            }
            bool inUse = player.channel && !player.noItems && !player.CCed;
            if (inUse)
            {
                anim ??= GetAnim();
                anim.Play(true);
            }
            else
            {
                Projectile.Kill();
            }
            Vector2 lookDir = player.LookDirection();
            player.direction = lookDir.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num32 = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * VisualLength * Projectile.scale, 32f * Projectile.scale, ref num32);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            string path = "ITD/Content/Projectiles/Unused/";
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glow = ModContent.Request<Texture2D>(path + "DespoticSword_Glow").Value;
            Texture2D effect = ModContent.Request<Texture2D>(path + "DespoticSword_Effect").Value;
            int length = Projectile.oldPos.Length;
            Vector2 extraHoldout = Projectile.velocity * 2f;
            for (int i = length - 1; i >= 0; i--)
            {
                Vector2 pos = Projectile.oldPos[i] + (Projectile.Size * 0.5f) - Main.screenPosition + extraHoldout;
                float rot = Projectile.oldRot[i];
                float prog = i / (float)(length - 1);
                Color col = Color.Lerp(Color.Aqua, Color.MidnightBlue, prog);
                float opac = 1f - prog;
                Main.EntitySpriteDraw(effect, pos, null, col * opac * FadeIn, rot + MathHelper.PiOver2, effect.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Vector2 drawPos = Projectile.Center + extraHoldout - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor * FadeIn, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(glow, drawPos, null, Color.White * FadeIn, Projectile.rotation + MathHelper.PiOver4, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
