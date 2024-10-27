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

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class DespoticSuperMeleeProj : ModProjectile
    {
        public ref float Scale => ref Projectile.ai[0];
        public override string Texture => "ITD/Content/Items/Weapons/Melee/DespoticSuperMeleeSword";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public void SpawnDust()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<DespoticDust>());
            }
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
            Player player = Main.player[Projectile.owner];
            Vector2 direction = new(player.direction, 0f);
            EntityAnim<Vector2> newAnim = Projectile.CreateAnim<Vector2>()
                .Append(() => Projectile.velocity, (direction * 32f).RotatedBy(-MathHelper.PiOver4), 20, InOutQuad)
                .Append(() => Projectile.velocity, (direction * 32f).RotatedBy(MathHelper.PiOver4), 20, InQuad);
            return newAnim;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
            Projectile.rotation = Projectile.velocity.ToRotation();
            player.heldProj = Projectile.whoAmI;
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
            player.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num32 = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 50f * Projectile.scale, 32f * Projectile.scale, ref num32);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center + Projectile.velocity - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
