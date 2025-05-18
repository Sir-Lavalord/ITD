using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.NPCs;
using ITD.Players;
using ITD.Utilities.Placeholders;
using Terraria.ID;
using ITD.Systems;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.Localization;

namespace ITD.Content.Items.Accessories.Combat.All
{
    public class OrionsRing : ModItem
    {
        public override string Texture => Placeholder.PHAxe;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<OrionsRingPlayer>().orionsRingA = true;
            if (player.GetModPlayer<OrionsRingPlayer>().activeBosses.Count <= 0)
            {
                player.GetDamage(DamageClass.Generic) += 0.05f;
            }
        }
    }

    public class OrionsRingPlayer : ModPlayer
    {
        public bool orionsRingA = false;

        public int timeSinceLastBossDeath = 0;
        public static float killTime = 300 * 60;
        public HashSet<int> activeBosses = new HashSet<int>();
        public bool timerActive = false;

        public override void ResetEffects()
        {
            orionsRingA = false;
            activeBosses.Clear();
        }
        public override void UpdateEquips()
        {
            if (!orionsRingA) return;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && NPCID.Sets.BossHeadTextures[npc.type] > -1)
                {
                    activeBosses.Add(npc.whoAmI);
                }
                Main.NewText((npc.type, npc.GetBossHeadTextureIndex()));
            }

            timerActive = activeBosses.Count > 1 && Player.active && Player.statLife > 0;

            if (timerActive)
            {
                timeSinceLastBossDeath++;
            }
            else
            {
                timeSinceLastBossDeath = 0;
            }

            if (timeSinceLastBossDeath >= killTime)
            {
                Player.KillMeCustom("OrionsRingTimeOut", 9999);
                timeSinceLastBossDeath = 0;
            }

            Player.GetDamage(DamageClass.Generic) += 0.15f * activeBosses.Count;
            Player.GetArmorPenetration(DamageClass.Generic) += 1 * activeBosses.Count;
            Player.statDefense += 3 * activeBosses.Count;


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!orionsRingA) return;

            if (NPCID.Sets.BossHeadTextures[target.type] > -1 && target.life <= 0)
            {
                timeSinceLastBossDeath = 0;
            }
        }
    }

    public class OrionsRingTimerUI : UIState
    {
        private float? fixedXPosition = null;
        private const float scale = 1.25f;
        private const int yPosition = 40;
        public static float killTime = 300 * 60;

        public override void Draw(SpriteBatch spriteBatch)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<OrionsRingPlayer>();

            if (modPlayer.timerActive && modPlayer.orionsRingA)
            {
                float timeLeft = (killTime - modPlayer.timeSinceLastBossDeath) / 60f;
                int minutes = (int)(timeLeft / 60);
                int seconds = (int)(timeLeft % 60);
                int milliseconds = (int)((timeLeft - (int)timeLeft) * 100);

                string timerText = $"{minutes:D2}:{seconds:D2}:{milliseconds:D2}";
                var font = Terraria.GameContent.FontAssets.DeathText.Value;
                if (!fixedXPosition.HasValue)
                {
                    Vector2 maxTextSize = font.MeasureString("99:99:99") * scale;
                    fixedXPosition = (Main.screenWidth - maxTextSize.X) * 0.5f;
                }

                Color textColor = timeLeft < 60 ?
                    Color.Lerp(Color.OrangeRed, Color.Red, (float)(Main.timeForVisualEffects * 0.05f % 1f)) :
                    Color.White;

                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    font,
                    timerText,
                    fixedXPosition.Value,
                    yPosition,
                    textColor,
                    Color.Black,
                    Vector2.Zero,
                    scale
                );
            }
            }
        }
    public class OrionsRingTimerUISystem : ModSystem
    {
        private UserInterface _timerInterface;
        internal OrionsRingTimerUI TimerUI;

        public override void Load()
        {
            TimerUI = new OrionsRingTimerUI();
            _timerInterface = new UserInterface();
            _timerInterface.SetState(TimerUI);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _timerInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Orion's Ring Timer",
                    delegate
                    {
                        _timerInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}