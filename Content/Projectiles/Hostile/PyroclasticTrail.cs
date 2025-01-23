using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile
{
    public class PyroclasticTrail : ModProjectile
    {
        private const int LifeTime = 160;
        private float ProgressOneToZero => Projectile.timeLeft / (float)LifeTime;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.width = 48;
            Projectile.height = 26;
            Projectile.timeLeft = LifeTime;
            Projectile.penetrate = -1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.4f; //gravity. i think this is the vanilla value for gravity?
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle baseHitbox = hitbox;
            int newHeight = (int)MathHelper.Lerp(baseHitbox.Height, 1, 1f - ProgressOneToZero);
            int newWidth = (int)MathHelper.Lerp(baseHitbox.Width, baseHitbox.Width * 2f, 1f - ProgressOneToZero);
            int newX = baseHitbox.Center.X - newWidth / 2;
            int newY = baseHitbox.Y + (baseHitbox.Height - newHeight);
            hitbox = new(newX, newY, newWidth, newHeight);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(tex.Width / 2, tex.Height / Main.projFrames[Type]); // bottom middle origin
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            //Main.spriteBatch.Draw(tex, Projectile.Bottom - Main.screenPosition, frame, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
            Rectangle hit = Projectile.Hitbox;
            ModifyDamageHitbox(ref hit);
            hit.Location -= Main.screenPosition.ToPoint();
            Main.spriteBatch.Draw(tex, hit, frame, Color.White);
            return false;
        }
    }
}
