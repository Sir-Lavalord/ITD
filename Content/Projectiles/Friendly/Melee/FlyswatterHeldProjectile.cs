using ITD.Utilities.EntityAnim;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;
using Terraria;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class FlyswatterHeldProjectile : ModProjectile
    {
        public override string Texture => "ITD/Content/Items/Weapons/Melee/Flyswatter";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public EntityAnim<Vector2> anim = null;
        public EntityAnim<Vector2> GetAnim()
        {
            return Projectile.CreateAnim<Vector2>()
                .Append(() => Projectile.velocity, FirstFrameDirection, 30, EasingFunctions.InOutQuad)
                .Append(() => Projectile.velocity, SecondFrameDirection, 10, EasingFunctions.InQuad);
        }
        public float GetRot()
        {
            return Main.player[Projectile.owner].LookDirection().ToRotation();
        }
        private Vector2 FrameDirection(int dirMultiplier)
        {
            int dir = Math.Sign(Main.player[Projectile.owner].direction) * dirMultiplier;
            return (Vector2.UnitX * 16f).RotatedBy(dir).RotatedBy(GetRot());
        }
        public Func<Vector2> FirstFrameDirection => () => FrameDirection(-1);
        public Func<Vector2> SecondFrameDirection => () => FrameDirection(1);
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
            Projectile.rotation = Projectile.velocity.ToRotation();
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
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
            Projectile.spriteDirection = player.direction = lookDir.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num32 = 0f;
            Vector2 center = Main.player[Projectile.owner].MountedCenter;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center, center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 64f, 1f, ref num32);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 extraHoldout = Projectile.velocity * 2f;
            Vector2 drawPos = Projectile.Center + extraHoldout - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.None);
            return false;
        }
    }
}
