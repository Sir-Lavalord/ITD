using Terraria.Audio;
using System;
using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Misc;
using System.Collections.Generic;

namespace ITD.Content.Items.Accessories.Movement.Jumps
{
    public class FirestormInABottle : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FirestormPlayer>().hasFireJump = true;
        }
    }

    public class FirestormPlayer : ModPlayer
    {
        public bool hasFireJump;
        public bool canDoubleJump;
        public bool waitDoubleJump;

        public override void ResetEffects() => hasFireJump = false;

        public override void PreUpdate()
        {
            if (!hasFireJump)
            {
                canDoubleJump = false;
            }

            HandleDoubleJump();
        }
        public override void PostUpdate()
        {
            if (!canDoubleJump && hasFireJump && Player.velocity.Y == 0)
            {
                SpawnFirestorm();
            }
        }

        private void HandleDoubleJump()
        {
            if ((Player.velocity.Y == 0f || Player.sliding || Player.autoJump && Player.justJumped) && hasFireJump)
            {
                canDoubleJump = true;
                waitDoubleJump = true;
            }
            else if (canDoubleJump && Player.controlJump && !waitDoubleJump)
            {
                PerformDoubleJump();
            }

            waitDoubleJump = Player.controlJump;
        }

        private void PerformDoubleJump()
        {
            if (Player.jump > 0) return;

            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.jump = Player.jumpHeight;
            canDoubleJump = false;
            SoundEngine.PlaySound(new SoundStyle($"ITD/Content/Sounds/FirestormInABottle{Main.rand.Next(1, 3)}"), Player.position);
        }
        private Vector2 FindGroundBelow(ref Vector2 position)
        {
            RaycastData data = Helpers.QuickRaycast(position, Vector2.UnitY, (point)
                => { return (!Main.tile[point].HasTile && Main.tile[point].TileType != TileID.Platforms) && Main.tile[point].IsActuated ; }, 300,false);
            return data.End + new Vector2(0,- 30);
        }
        private void SpawnFirestorm()
        {
            int pillarCount = 8;
            float radius = 150f;

            for (int i = 0; i < pillarCount; i++)
            {
                Vector2 position = Player.Top + new Vector2(
                    Main.rand.NextFloat(-radius, radius),
                    0
                );

                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    FindGroundBelow(ref position),
                    Vector2.Zero,
                    ModContent.ProjectileType<FirestormPillar>(),
                    50,
                    5f,
                    Player.whoAmI,i * Main.rand.Next(10,30)
                );

                for (int j = 0; j < 10; j++)
                {
                    Dust.NewDust(position, 10, 10, DustID.RedTorch);
                }
            }
        }
    }
}