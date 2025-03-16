using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Systems;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Creative;
using System.Linq;
using ITD.Content.Projectiles.Friendly.Ranger;
using Terraria.GameContent.Drawing;
using ITD.Content.Projectiles.Other;
using ITD.Content.Projectiles;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Potshot : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White * 0.7f
            });
            FrontGunLayer.RegisterData(Item.type);
        }
        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 45;
            Item.reuseDelay = 40;
            Item.useStyle = -1;
            Item.noMelee = true;
            Item.knockBack = 0.1f;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = 1;
            Item.shootSpeed = 6f;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player)
        {
            ITDPlayer itdPlayer = player.GetITDPlayer();
            NPC[] npcs = itdPlayer.GetNearbyNPCs(30f * 16f);
            if (npcs.Length > 0)
            {

                    NPC target = npcs.OrderByDescending(npc => npc.Distance(player.Center)).LastOrDefault();
                if (player.ownedProjectileCounts[ModContent.ProjectileType<PotshotReticle>()] < 1)
                {
                    Vector2 FakeMountedCenter = player.MountedCenter;
                    FakeMountedCenter.Y -= 5;
                    Vector2 vector2 = player.RotatedRelativePoint(FakeMountedCenter, true);
                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.FlameWaders, new ParticleOrchestraSettings
                    {
                        PositionInWorld = vector2,
                    }, player.whoAmI);

                    Projectile.NewProjectile(player.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PotshotReticle>(), 0, 0, player.whoAmI);
                }
            }
            if (player.altFunctionUse != 2)
            {
                if (miss >= 0.3f)
                {
                    miss = 0;
                }
            }
            return true;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White * 0.7f, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
        float miss;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(Main.rand.NextBool(2) == true?
                new SoundStyle("ITD/Content/Sounds/PotshotFire1"):
                new SoundStyle("ITD/Content/Sounds/PotshotFire2")
                , position);
            Vector2 FakeMountedCenter = player.MountedCenter;
            FakeMountedCenter.Y -= 5;
            Vector2 vector2 = player.RotatedRelativePoint(FakeMountedCenter, true);
            Vector2 bulletVel = velocity;
            bulletVel.Normalize();
            bulletVel *= 18f + (miss * 10);
            Vector2 muzzle = Vector2.Normalize(velocity) * 2;
            if (Collision.CanHit(vector2, 0, 0, vector2 + muzzle, 0, 0))
            {
                vector2 += muzzle;
            }
            for (int i = 0; i < 4; i++)
            {
                float piOffsetAmt = (float)i - 3f / 2f;
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(miss * 150));
                newVelocity *= 1f - Main.rand.NextFloat(miss);
                Vector2 offsetSpawn = bulletVel.RotatedBy((double)((MathHelper.Pi/10) * piOffsetAmt), default);
                int proj = Projectile.NewProjectile(source, vector2.X + offsetSpawn.X, vector2.Y + offsetSpawn.Y, newVelocity.X, newVelocity.Y, type, damage, knockback, player.whoAmI);
                Main.projectile[proj].GetGlobalProjectile<ITDInstancedGlobalProjectile>().isFromPotshot = true;

            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(miss * 60));
                newVelocity *= Main.rand.NextFloat(2f);
                Dust dust = Dust.NewDustDirect(player.MountedCenter, Item.width/2, Item.height/2, DustID.Smoke, 0f, 0f, 0, default, 2f);
                dust.noGravity = true;
                dust.velocity = -newVelocity;
                Dust dust2 = Dust.NewDustDirect(player.MountedCenter, Item.width / 2, Item.height / 2, DustID.Torch, 0f, 0f, 0, default, 2f);
                dust2.noGravity = true;
                dust2.velocity = -newVelocity;
            }
            miss += 0.1f;
            ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            modPlayer.recoilFront = 0.1f + (miss);

            return false;
        }

        public void Hold(Player player)
        {
            if (!player.ItemAnimationActive)
            {
                miss = 0;
            }
            ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
                player.direction = -1;
            else
                player.direction = 1;

            float rotation = (Vector2.Normalize(mouse - player.MountedCenter) * player.direction).ToRotation();
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(10f, -9f);
        }
    }
}
