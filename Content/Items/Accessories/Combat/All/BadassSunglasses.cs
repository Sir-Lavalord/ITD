using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;

namespace ITD.Content.Items.Accessories.Combat.All;

[AutoloadEquip(EquipType.Face)]
public class BadassSunglasses : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.value = Item.buyPrice(10);
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        BadassPlayer modPlayer = player.GetModPlayer<BadassPlayer>();
        modPlayer.sunglassesOn = true;

        if (!hideVisual)
            modPlayer.sunglassesVanity = true;

        if (modPlayer.sunglassesCharge < 200)
            modPlayer.sunglassesCharge++;
        player.GetCritChance(DamageClass.Generic) += Math.Max(modPlayer.sunglassesCharge - 100, 0);
    }

    public override void UpdateVanity(Player player)
    {
        BadassPlayer modPlayer = player.GetModPlayer<BadassPlayer>();
        modPlayer.sunglassesVanity = true;
    }
}

public class BadassPlayer : ModPlayer
{
    public bool sunglassesOn;
    public bool sunglassesVanity;
    public int sunglassesCharge = 0;

    public override void ResetEffects()
    {
        if (!sunglassesOn)
            sunglassesCharge = 0;
        sunglassesOn = false;
        sunglassesVanity = false;
    }
    public override void UpdateDead()
    {
        sunglassesCharge = 0;
        sunglassesVanity = false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.life > 0)
        {
            if (sunglassesCharge > 100)
            {
                SoundEngine.PlaySound(SoundID.Item101, Player.Center);
                for (int i = 0; i < 15; ++i)
                {
                    int dustId = Dust.NewDust(Player.Center, 0, 0, DustID.GoldFlame, 0.0f, 0.0f, 100, new Color(), 2f);
                    Main.dust[dustId].noGravity = true;
                    Main.dust[dustId].velocity *= 5f;
                }
            }
            sunglassesCharge = 0;
        }
        else
        {
            if (sunglassesCharge > 100)
            {
                SoundEngine.PlaySound(SoundID.Item94, Player.Center);
                string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(BadassSunglasses)}.KillMessages.{Main.rand.Next(3)}")).Value;
                CombatText.NewText(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), Color.Yellow, Text);
            }
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (sunglassesVanity && drawInfo.shadow == 0f)
        {
            Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(Player.width * 0.5f, Player.height * 0.5f - 8f * Player.gravDir);
            Asset<Texture2D> texture = Mod.Assets.Request<Texture2D>("Content/Items/Accessories/Combat/All/BadassSunglasses_Aura");
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color color = Color.White;
            float opacity;
            if (sunglassesOn)
                opacity = Math.Max(sunglassesCharge - 100, 0) * 0.01f;
            else
                opacity = 1f;
            color.A = (byte)(color.A * 0.5f);
            color *= opacity;

            int scale1 = (int)(Main.GlobalTimeWrappedHourly * 30) % 30;
            int scale2 = (int)(Main.GlobalTimeWrappedHourly * 30 + 10) % 30;
            int scale3 = (int)(Main.GlobalTimeWrappedHourly * 30 + 20) % 30;

            Main.CurrentDrawnEntityShader = Player.cFace;

            SpriteEffects effects;
            if (Player.gravDir == 1)
                effects = SpriteEffects.None;
            else
                effects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture.Value, position, sourceRectangle, color * ((30 - scale1) * 0.025f), 0, origin, 0.6f + scale1 * 0.02f, effects, 0f);
            Main.EntitySpriteDraw(texture.Value, position, sourceRectangle, color * ((30 - scale2) * 0.025f), 0, origin, 0.6f + scale2 * 0.02f, effects, 0f);
            Main.EntitySpriteDraw(texture.Value, position, sourceRectangle, color * ((30 - scale3) * 0.025f), 0, origin, 0.6f + scale3 * 0.02f, effects, 0f);
            Main.CurrentDrawnEntityShader = 0;
        }
    }
}