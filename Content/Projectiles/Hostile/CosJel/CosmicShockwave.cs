using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicShockwave : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            Projectile.light = 1;
            Projectile.timeLeft = 200;

            Projectile.hide = true;
        }
        public override void AI()
        {
            if (Projectile.ai[1]++ < 20)
            {
                Projectile.velocity *= 0.99f;
                Projectile.netUpdate = true;
            }
            else if (Projectile.ai[1] >= 30 && Projectile.ai[1] <= 70)
            {
                if (Math.Abs(Projectile.velocity.X) <= 18)
                Projectile.velocity *= 1.08f;
                Projectile.netUpdate = true;
            }
            else if (Projectile.ai[1] > 150)
            {
                Projectile.velocity *= 0.95f;
                if (Projectile.alpha < 160)
                    Projectile.alpha += 3;
                else Projectile.Kill();
            }
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.hide)
                behindNPCsAndTiles.Add(index);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 stretch = new(1f, 1f);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            stretch = new Vector2(1f + Projectile.velocity.Length() * 0.05f,1f);

            float rotation = Projectile.rotation;
            Vector2 offset = Vector2.UnitY * -10;
            Vector2 drawPos = Projectile.Center + offset;

            int sizeY = tex.Height / Main.projFrames[Type];
            int frameY = Projectile.frame * sizeY;
            Rectangle rectangle = new(0, frameY, tex.Width, sizeY);
            Vector2 origin = rectangle.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                Color oldColor = Color.White;
                oldColor *= 0.5f;
                oldColor *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2 + offset;
                float oldRot = Projectile.oldRot[i];
                Main.EntitySpriteDraw(tex, oldPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Projectile.GetAlpha(oldColor),
                    oldRot, origin, stretch, spriteEffects, 0);
            }
            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;

            time %= 4f;
            time /= 2f;

            if (time >= 1f)
            {
                time = 2f - time;
            }

            time = time * 0.5f + 0.5f;

            for (float i = 0f; i < 1f; i += 0.25f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 4f).RotatedBy(radians) * time, rectangle, new Color(90, 70, 255, 50), Projectile.rotation, origin, stretch, Projectile.spriteDirection == 1 ? SpriteEffects.None: SpriteEffects.FlipHorizontally, 0);
            }

            for (float i = 0f; i < 1f; i += 0.34f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;

                Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 6f).RotatedBy(radians) * time, rectangle, new Color(90, 70, 255, 50), Projectile.rotation, origin, stretch, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Projectile.GetAlpha(Color.White),
                    rotation, origin, stretch, spriteEffects, 0);

            return false;
        }
    }
}