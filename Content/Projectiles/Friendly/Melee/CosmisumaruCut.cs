using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
using ITD.Content.Tiles.Misc;
using ITD.Content.Dusts;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CosmisumaruCut : ModProjectile
    {
        public int attackRate;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 108;
            Projectile.height = 52;
            Projectile.scale = 1.2f;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.light = 0.1f;

            Projectile.noEnchantmentVisuals = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            target.immune[Projectile.owner] = 8;

            player.GetITDPlayer().charge += damageDone;
        }

        public override void AI()
        {

            Player player = Main.player[Projectile.owner]; 
            float num = 1.57079637f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, true); 
            if (player.direction > 0)
            {
                DrawOffsetX = +0;
                DrawOriginOffsetX = -0;
                DrawOriginOffsetY = +0;
            }
            else if (player.direction < 0)
            {
                DrawOffsetX = -0;
                DrawOriginOffsetX = +20;
                DrawOriginOffsetY = +0;
            }
            num = 0f; 
            if (Projectile.spriteDirection == -1)
            {
                num = 3.14159274f;
            }
            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 10)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.soundDelay--; 
            
            if (Main.myPlayer == Projectile.owner)
            {
                if (player.channel && !player.noItems && !player.CCed) 
                {                                                                                       
                    float scaleFactor6 = 1f;
                    if (player.inventory[player.selectedItem].shoot == Projectile.type) 
                    {                                                                                                  
                        scaleFactor6 = player.inventory[player.selectedItem].shootSpeed * Projectile.scale;
                    }
                    Vector2 vector13 = Main.MouseWorld - vector; 
                    vector13.Normalize();
                    if (vector13.HasNaNs()) 
                    {
                        vector13 = Vector2.UnitX * (float)player.direction; 
                    }
                    vector13 *= scaleFactor6;
                    if (vector13.X != Projectile.velocity.X || vector13.Y != Projectile.velocity.Y)
                    {                                                                                                       
                        Projectile.netUpdate = true; 
                    }
                    Projectile.velocity = vector13; 
                }
                else
                {
                    Projectile.Kill();
                }
            }
            Vector2 vector14 = Projectile.Center + Projectile.velocity * 2f; 
                                  
            if (Main.rand.NextBool(3))
            {         
                int num30 = Dust.NewDust(vector14 - Projectile.Size / 2f, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), Projectile.velocity.X, Projectile.velocity.Y, 0, default(Color), 0.5f);
                Main.dust[num30].noGravity = true;
                Main.dust[num30].position -= Projectile.velocity;
            }

            Projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f; 
            Projectile.rotation = Projectile.velocity.ToRotation() + num; 
                                                                     
            Projectile.spriteDirection = Projectile.direction; 
            Projectile.timeLeft = 2; 
            player.ChangeDir(Projectile.direction);
                                                  
            player.heldProj = Projectile.whoAmI;
                                                 
            player.itemTime = 20; 
            player.itemAnimation = 20;
        }
    }
}