using ITD.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class DunebarrelRotate : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            // Prevents jitter when stepping up and down blocks and half blocks
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1; // Replace with 20 if you do not want custom code
            Projectile.hide = true; // Hides the projectile, so it will draw in the player's hand when we set the player's heldProj to this one.
            Projectile.damage = 0;
            Projectile.scale = 0.75f;
            Projectile.timeLeft = 30;
        }
        float fHoldoutDistance;
        Vector2 vHoldoutOffset;
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            //Peak hardcode retardation

            fHoldoutDistance = player.HeldItem.shootSpeed * Projectile.scale;
            vHoldoutOffset = fHoldoutDistance * Vector2.Normalize(Main.MouseWorld - player.Center);
        }
        int iSpindex;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                if (Main.MouseWorld.X >= player.Center.X)
                {
                    player.direction = -1;
                }
                else if (Main.MouseWorld.X < player.Center.X)
                {
                    player.direction = -1;

                }
                Projectile.rotation += (MathHelper.ToRadians(14) * player.direction);
                Projectile.spriteDirection = player.direction;
                player.ChangeDir(Projectile.direction);
                player.heldProj = Projectile.whoAmI;
                Projectile.Center = player.MountedCenter;
                Projectile.Center -= vHoldoutOffset;
                //ffs this is hardcoded to the extreme, if anyone could think of a better fix, feel free to do so
            }

        }
        public static Vector2 Gun2Position { get; private set; }
        public static void SetPosition(Vector2 position)
        {
            Gun2Position = position;
        }
        //For some goddamn reason, the thing doesn't work
/*        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Color color26 = lightColor;
            //Why tf does the thing rotate from the barrel
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle),
                Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
            return false;
        }*/

    }
}