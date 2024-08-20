using ITD.Content.Projectiles;
using Microsoft.Xna.Framework;
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
    public class DunebarrelRightGun : ModProjectile
    {
        public static Vector2 Gun1Position { get; private set; }
        private int GunToShoot;
        private float fireTiltRotation;
        public override void SetStaticDefaults()
        {
            // Prevents jitter when stepping up and down blocks and half blocks
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1; // Replace with 20 if you do not want custom code
            Projectile.hide = true; // Hides the projectile, so it will draw in the player's hand when we set the player's heldProj to this one.
            Projectile.scale = 0.65f;
            Projectile.damage = 0;

            GunToShoot = 1;
        }

        // This code is adapted and simplified from aiStyle 20 to use a different dust and more noises. If you want to use aiStyle 20, you do not need to do any of this.
        // It should be noted that this projectile has no effect on mining and is mostly visual.
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Gun1Position = player.itemLocation;
            Projectile.timeLeft = 60;

            // Animation code could go here if the projectile was animated. 

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            if (Main.myPlayer == Projectile.owner)
            {
                // This code must only be ran on the client of the projectile owner
                if (player.channel)
                {
                    float holdoutDistance = player.HeldItem.shootSpeed * Projectile.scale;
                    // Calculate a normalized vector from player to mouse and multiply by holdoutDistance to determine resulting holdoutOffset
                    Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);
                    if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y)
                    {
                        // This will sync the projectile, most importantly, the velocity.
                        Projectile.netUpdate = true;
                    }

                    // Projectile.velocity acts as a holdoutOffset for held projectiles.
                    Projectile.velocity = holdoutOffset;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
            }
            else if (Projectile.velocity.X < 0f)
            {
                player.ChangeDir(-1);
            }

            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            Projectile.Center = playerCenter;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.ai[1]++;
                int interval = (int)(8 / player.GetTotalAttackSpeed(DamageClass.Ranged));
                if (Projectile.ai[1] > interval)
                {
                    Projectile.ai[1] = 0;

                    Item item = player.HeldItem;


                    if (!player.PickAmmo(item, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId))
                    {
                        return;
                    }

                    var source = player.GetSource_ItemUse_WithPotentialAmmo(item, usedAmmoItemId);
                    Vector2 vec = Main.MouseWorld;
                    Vector2 vector = vec - Projectile.Center;
                    float mag = vector.Length();
                    if (mag > speed)
                    {
                        mag = speed / mag;
                        vector *= mag;
                    }

                    Vector2 perturbedSpeed = vector.RotatedByRandom(MathHelper.ToRadians(2));
                    vector = perturbedSpeed;

                    Vector2 finalvector = vector * 0.12f;  

                    if (GunToShoot == 1)
                    {
                        Projectile.NewProjectile(source, Projectile.Center, finalvector, projToShoot, damage, knockBack, Projectile.owner);
                        GunToShoot = 2;
                    }
                    else
                    {
                        Projectile.NewProjectile(source, Gun1Position, finalvector, projToShoot, damage, knockBack, Projectile.owner);
                        GunToShoot = 1;
                    }
                }
            }
        }
    }
}

          

          