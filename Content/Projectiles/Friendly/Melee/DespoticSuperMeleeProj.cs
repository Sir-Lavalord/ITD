using ITD.Utilities;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ITD.Content.Dusts;
using Terraria.ID;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class DespoticSuperMeleeProj : ModProjectile
    {
        public ref float Scale => ref Projectile.ai[0];
		public ref float direction => ref Projectile.ai[1];
		public int maxTime = 30;
        public override string Texture => "ITD/Content/Items/Weapons/Melee/DespoticSuperMeleeSword";
        public const float VisualLength = 90f;
		
		public static VertexStrip vertexStrip = new VertexStrip();
		
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
			Projectile.timeLeft = 30;
            Projectile.Opacity = 0f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = Scale;
            Projectile.width = (int)(Projectile.width * Scale);
            Projectile.height = (int)(Projectile.height * Scale);
		}
        public override void AI()
        {
			if (Projectile.timeLeft < 5)
				Projectile.Opacity -= 0.2f;
			else if (Projectile.Opacity < 1f)
                Projectile.Opacity += 0.2f;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.MountedCenter;
			Projectile.velocity = Projectile.velocity.RotatedBy(0.3f * Projectile.timeLeft/maxTime * direction);
			
			Projectile.rotation = Projectile.velocity.ToRotation();
			
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) // custom trailing
			{
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
			}
			Projectile.oldPos[0] = Projectile.Center + Projectile.velocity * 3f + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 36f;
			Projectile.oldRot[0] = Projectile.rotation + MathHelper.PiOver2;
           		   
            player.heldProj = Projectile.whoAmI;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num32 = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * VisualLength * Projectile.scale, 32f * Projectile.scale, ref num32);
        }
		
		private Color StripColors(float progressOnStrip)
		{
			Color result = Color.Lerp(Color.Aqua, Color.MidnightBlue, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
			result.A /= 2;
			return result * Projectile.Opacity * Projectile.Opacity;
		}
		private float StripWidth(float progressOnStrip)
		{
			return 64f;
		}
        public override bool PreDraw(ref Color lightColor)
        {
            string path = "ITD/Content/Projectiles/Friendly/Melee/";
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glow = ModContent.Request<Texture2D>(path + "DespoticSword_Glow").Value;
			MiscShaderData arg_27_0 = GameShaders.Misc["FinalFractal"];
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			int num4 = 4;
			arg_27_0.UseShaderSpecificData(new Vector4((float)num, (float)num2, (float)num3, (float)num4));
			arg_27_0.UseImage0("Images/Extra_" + 201);
			arg_27_0.UseImage1("Images/Extra_" + 193);
			arg_27_0.Apply(null);
			vertexStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition, new int?(Projectile.oldPos.Length), true);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
			
			Vector2 extraHoldout = Projectile.velocity * 2f;
            Vector2 drawPos = Projectile.Center + extraHoldout - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(glow, drawPos, null, Color.White * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
			
            return false;
        }
    }
}
