using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.NPCs.Friendly;
namespace ITD.Content.Items.Accessories.Master
{
    public class SlimeSecretionGland : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(8, 5));
            Item.ResearchUnlockCount = 1;

        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<KSGlandPlayer>().ksMasterAcc = true;
        }
    }
    public class KSGlandPlayer : ModPlayer
    {
        public bool ksMasterAcc;
        public int RegrowCD = 0;
        public override void ResetEffects()
        {
            ksMasterAcc = false;
        }
        public override void UpdateDead()
        {
            RegrowCD = 0;
        }
        public override void PostUpdateEquips()
        {
            if (ksMasterAcc && (RegrowCD <= 0))
            {
                if (!Player.isNearNPC(ModContent.NPCType<KSGlandNPC>()))
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {

                        NPC.NewNPCDirect(Player.GetSource_FromThis(), Player.Center, ModContent.NPCType<KSGlandNPC>(), 0, Player.whoAmI);
                    }
                }
            }
            if (RegrowCD >= 0)
            {
                RegrowCD--;
            }
            else
            {
                RegrowCD = 0;
            }
        }
    }
}

