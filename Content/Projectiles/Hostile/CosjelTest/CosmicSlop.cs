using System;
using System.Collections.Generic;
using System.Linq;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ITD.Content.Projectiles.Hostile.CosjelTest
{
    public class CosmicSlop : Deathray
    {
        public override string Texture => "FargowiltasSouls/Content/Projectiles/Deathrays/GolemBeam";
        public CosmicSlop() : base(300) { }

        // Setting up static defaults for the projectile
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        // Set specific defaults for this projectile
        public override void SetDefaults()
        {
            base.SetDefaults();

            // Customizing the size of the projectile
            Projectile.width = 64; // Wider than the default
            Projectile.height = 64;
            Projectile.penetrate = -1; // Allow it to pass through multiple targets
            Projectile.tileCollide = true; // Allow the deathray to collide with tiles and platforms
        }

        // Adjusting AI (behavior of the projectile as it moves)
        public override void AI()
        {
            // Customizing behavior: Example of a small movement change (just for illustration)
            if (Projectile.timeLeft < 240)
            {
                Projectile.alpha = 255 - (int)(255f * (Projectile.timeLeft / 240f)); // Fade out as it nears the end of its life
            }

            // Call to parent AI method
            base.PostAI();
        }

        // Custom collision logic (inheriting from the abstract class and using its logic)
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool? result = base.Colliding(projHitbox, targetHitbox); // Use the default collision behavior for entities

            // Add custom collision with tiles
            if (Projectile.tileCollide)
            {
                Vector2 start = Projectile.Center;
                Vector2 end = start + Projectile.velocity * Projectile.localAI[1]; // Length of the ray
                float width = Projectile.width * Projectile.scale * hitboxModifier;
                Vector2? collisionResult = Collision.TileCollision(start, end, (int)width, (int)width);
                if (collisionResult.HasValue)
                {
                    return true; // Collision occurred
                }
            }

            return result;
        }

        // Custom CutTiles logic, inherited from the base class
        public override void CutTiles()
        {
            // Delegate method for cutting tiles when the deathray intersects them
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 unit = Projectile.velocity;
            Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * Projectile.localAI[1], Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
        }

        Rectangle Frame(Texture2D tex)
        {
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            return new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
            {
                return false;
            }

            SpriteEffects spriteEffects = SpriteEffects.None;

            Texture2D texture2D19 = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D20 = ModContent.Request<Texture2D>($"{Texture}2", AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D21 = ModContent.Request<Texture2D>($"{Texture}3", AssetRequestMode.ImmediateLoad).Value;

            float rayLength = Projectile.localAI[1];
            Texture2D arg_ABD8_1 = texture2D19;
            Vector2 arg_ABD8_2 = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle2 = texture2D19.Bounds;
            Main.EntitySpriteDraw(arg_ABD8_1, arg_ABD8_2, sourceRectangle2, Projectile.GetAlpha(lightColor), Projectile.rotation, sourceRectangle2.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            rayLength -= (texture2D19.Height / 2 + texture2D21.Height) * Projectile.scale;
            Vector2 value20 = Projectile.Center;
            value20 += Projectile.velocity * Projectile.scale * texture2D19.Height / 2f;
            if (rayLength > 0f)
            {
                float num224 = 0f;
                Rectangle rectangle7 = texture2D20.Bounds;
                while (num224 < rayLength)
                {
                    Main.EntitySpriteDraw(texture2D20, value20 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle7), Projectile.GetAlpha(Lighting.GetColor((int)value20.X / 16, (int)value20.Y / 16)), Projectile.rotation, rectangle7.Size() / 2, Projectile.scale, spriteEffects, 0);
                    num224 += rectangle7.Height * Projectile.scale;
                    value20 += Projectile.velocity * rectangle7.Height * Projectile.scale;
                }
            }
            Texture2D arg_AE2D_1 = texture2D21;
            Vector2 arg_AE2D_2 = value20 - Main.screenPosition;
            sourceRectangle2 = texture2D21.Bounds;
            Main.EntitySpriteDraw(arg_AE2D_1, arg_AE2D_2, sourceRectangle2, Projectile.GetAlpha(Lighting.GetColor((int)value20.X / 16, (int)value20.Y / 16)), Projectile.rotation, sourceRectangle2.Size() / 2, Projectile.scale, spriteEffects, 0);
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            NPC npc = MiscHelpers.NPCExists(Projectile.ai[1], ModContent.NPCType<CosmicJellyfish>());

            Player player = Main.player[npc.target];
                width = 15;
                height = 15;
                fallThrough = player.Center.Y >= Projectile.Bottom.Y + 20;
            
            return true;

        }
    }
}