using ITD.Content.Projectiles.Friendly.Misc;
using System;
using Terraria.DataStructures;
namespace ITD.Content.Items.Accessories.Movement.Jumps;

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
        player.GetJumpState<FirestormJump>().Enable();
    }
}
public class FirestormJump : ExtraJump
{
    public override Position GetDefaultPosition() => new Before(CloudInABottle);

    /*        public override IEnumerable<Position> GetModdedConstraints()
            {
                yield return new Before(ModContent.GetInstance<MultipleUseExtraJump>());
            }
    */
    public override float GetDurationMultiplier(Player player)
    {
        return 1.5f;
    }
    public int pillarCount = 0;
    public int pillarCountMax = 5;
    public override void UpdateHorizontalSpeeds(Player player)
    {
        player.runAcceleration *= 1;
        player.maxRunSpeed *= 1f;

    }
    public override void OnRefreshed(Player player)
    {
        // Reset the jump counter
        player.GetModPlayer<FirestormPlayer>().pillarCount = 0;
        player.GetModPlayer<FirestormPlayer>().fireJumped = false;

    }

    public override void OnStarted(Player player, ref bool playSound)
    {
        player.GetModPlayer<FirestormPlayer>().fireJumped = true;
        for (int j = 0; j < 20; j++)
        {
            Dust dust = Dust.NewDustDirect(player.Bottom + new Vector2(-20, 0), player.width * 2, 10, DustID.Torch);
            dust.noGravity = true;
            dust.scale = 2f * Main.rand.NextFloat(0.75f, 1.25f);
            dust.velocity.X = 4 * (j % 2 == 0 ? 1 : -1) * Main.rand.NextFloat(0.25f, 1.25f);
        }

    }
    public override void OnEnded(Player player)
    {
    }
    public override void ShowVisuals(Player player)
    {
        for (int i = 0; i < 2; i++)
        {
            int direction = i == 0 ? 1 : -1;

            Vector2 position = player.Center + new Vector2(direction * 12f, (player.height + 2f) * player.gravDir * 0.5f);

            Dust dust = Dust.NewDustDirect(position - new Vector2(4f, 4f), 0, 0, DustID.Torch, 0f, 0f, 0, default, 3f);
            //dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cShoe, Player);
            dust.noGravity = true;
            dust.velocity = player.velocity / 3 + new Vector2(0,
                6f).RotatedByRandom(MathHelper.ToRadians(10));
        }
    }
}
public class FirestormPlayer : ModPlayer
{
    public bool hasFireJump = false;
    public bool fireJumped = false;
    public int pillarCount = 0;
    public int pillarCountMax = 5;

    public override void ResetEffects()
    {
        hasFireJump = false;
    }
    public override void PostUpdate()
    {
        if (fireJumped && hasFireJump)
        {
            if (Player.velocity.Y == 0 || (pillarCount > 0 && pillarCount < pillarCountMax))
            {
                while (pillarCount < pillarCountMax)
                {
                    SpawnFirestorm();
                }
            }
            for (int i = 0; i < 2; i++)
            {
                int direction = i == 0 ? 1 : -1;

                Vector2 position = Player.Center + new Vector2(direction * 12f, (Player.height + 2f) * Player.gravDir * 0.5f);

                Dust dust = Dust.NewDustDirect(position - new Vector2(4f, 4f), 0, 0, DustID.Torch, 0f, 0f, 0, default, 2f);
                //dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cShoe, Player);
                dust.noGravity = true;
                dust.velocity = Player.velocity / 2 + new Vector2(0, -Player.gravDir * 6f).RotatedByRandom(MathHelper.ToRadians(10));
            }
        }
    }

    private static Vector2 FindGroundBelow(ref Vector2 position)
    {
        RaycastData data = Helpers.QuickRaycast(position, Vector2.UnitY, (point) =>
        {
            Tile t = Main.tile[point];
            return !t.HasTile && !Main.tileSolidTop[t.TileType] && t.IsActuated;
        }, 
        300);
        return data.End + new Vector2(0, -30);
    }
    private void SpawnFirestorm()
    {
        float radius = 150f;


        Vector2 position = Player.Center + new Vector2(
            Main.rand.NextFloat(-radius, radius),
            0
        );
        if (Math.Abs(position.Y - FindGroundBelow(ref position).Y) >= 50)
        {
            return;
        }

        Projectile.NewProjectile(
            Player.GetSource_FromThis(),
            FindGroundBelow(ref position),
            Vector2.Zero,
            ModContent.ProjectileType<FirestormPillar>(),
            30,
            5f,
            Player.whoAmI, pillarCount * 25
        );

        pillarCount++;
        for (int j = 0; j < 12; j++)
        {
            Dust dust = Dust.NewDustDirect(Player.Bottom + new Vector2(-20, 0), Player.width * 2, 10, DustID.Torch);
            dust.noGravity = true;
            dust.scale = 1.5f * Main.rand.NextFloat(0.75f, 1.25f);
            dust.velocity.X = 10 * (j % 2 == 0 ? 1 : -1) * Main.rand.NextFloat(0.25f, 1.25f);
        }
    }
    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (hasFireJump && fireJumped && drawInfo.shadow == 0f)
        {
            Texture2D texture = Mod.Assets.Request<Texture2D>("Content/Items/Accessories/Movement/Jumps/FirestormInABottle_Fire").Value;
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;

            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;
            time %= 4f;
            time /= 2f;

            if (time >= 1f)
            {
                time = 2f - time;
            }

            time = time * 0.5f + 0.5f;
            for (int i = 0; i < 2; i++)
            {
                int direction = i == 0 ? 1 : -1;
                Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(Player.width * 0.5f + direction * 12f, Player.height * (Player.gravDir == 1 ? 1 : 0) - 4f * Player.gravDir);
                float sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10);
                // Vector2 miragePos = position - Main.screenPosition + origin;

                for (float f = 0f; f < 1f; f += 0.3f)
                {
                    float radians = (f + timer) * MathHelper.TwoPi;

                    Main.EntitySpriteDraw(texture, position + new Vector2(0f, 4).RotatedBy(radians) * time, sourceRectangle, new Color(255, 130, 41, 100), default, origin, 0.75f, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 130, 41, 50), default, origin, 0.75f + sine * 0.05f, drawInfo.itemEffect, 0f);

            }
        }
    }
}