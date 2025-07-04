using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Utilities;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class TwilightDemiseProj : ITDProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 900;
            Projectile.light = 0.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }
        //equip a dye in pet slot to see bug
        public override int ProjectileShader(int originalShader)
        {
            return GameShaders.Armor.GetShaderIdFromItemId(ItemID.TwilightDye);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            DrawAtProj(outline);
            DrawAtProj(texture);
            return false;
        }
    }
}
