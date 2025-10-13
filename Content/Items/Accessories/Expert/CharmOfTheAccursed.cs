using ITD.Content.Buffs.Debuffs;
using ITD.Content.NPCs;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Accessories.Expert;

public class CharmOfTheAccursed : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.value = Item.sellPrice(50000);
        Item.expert = true;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CharmOfTheAccursedPlayer modPlayer = player.GetModPlayer<CharmOfTheAccursedPlayer>();
        modPlayer.Accursed = !hideVisual;

        foreach (var npc in Main.ActiveNPCs)
        {
            float distance = Vector2.Distance(npc.Center, player.Center);
            if (distance < 225 && !npc.friendly && npc.CanBeChasedBy() && !npc.HasBuff(ModContent.BuffType<HauntedBuff>()))
            {
                ITDGlobalNPC globalNPC = npc.GetGlobalNPC<ITDGlobalNPC>();
                globalNPC.haunting = true;
                globalNPC.hauntingProgress++;

                if (globalNPC.hauntingProgress % 2 == 0)
                {
                    Vector2 offset = new();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * 100);
                    offset.Y += (float)(Math.Cos(angle) * 100);
                    Vector2 spawnPos = npc.Center + offset - new Vector2(4, 0);
                    Dust dust = Main.dust[Dust.NewDust(
                        spawnPos, 0, 0,
                        DustID.SteampunkSteam, 0, 0, 0, default, 1.5f
                        )];
                    dust.velocity = -offset * 0.08f;
                    dust.noGravity = true;
                }

                if (globalNPC.hauntingProgress > 120)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath51, npc.Center);
                    for (int i = 0; i < 15; ++i)
                    {
                        int dustId = Dust.NewDust(npc.Center, 0, 0, DustID.SteampunkSteam, 0, 0, 0, new Color(), 2f);
                        Main.dust[dustId].noGravity = true;
                        Main.dust[dustId].velocity *= 3f;
                    }
                    npc.AddBuff(ModContent.BuffType<HauntedBuff>(), 1200);
                    globalNPC.hauntingProgress = 0;
                }
            }
        }
    }
}

public class CharmOfTheAccursedPlayer : ModPlayer
{
    public bool Accursed;
    public override void ResetEffects()
    {
        Accursed = false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.HasBuff(ModContent.BuffType<HauntedBuff>()))
        {
            Player.ManaEffect(1);
            Player.statMana++;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (Accursed && drawInfo.shadow == 0f)
        {
            Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(Player.width * 0.5f, Player.height * 0.5f);
            Asset<Texture2D> texture = Mod.Assets.Request<Texture2D>("Content/Items/Accessories/Expert/CharmOfTheAccursed_Aura");
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color color = Color.White;

            for (int i = 0; i < 8; ++i)
            {
                float rotation = Main.GlobalTimeWrappedHourly + i * MathHelper.PiOver4;
                Vector2 offset = (-Vector2.UnitY * 225).RotatedBy(rotation);

                Main.EntitySpriteDraw(texture.Value, position + offset, sourceRectangle, color, rotation, origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
