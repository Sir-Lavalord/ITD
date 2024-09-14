using ITD.Content.UI;
using ITD.Systems;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ITD.Content.Items.Favors
{
    public enum ChargeType
    {
        DamageGiven,
        DamageTaken,
        DistanceTravelled,
        EnemiesKilled,
    }
    public struct ChargeData(ChargeType type, NPC npc, Projectile projectile, float amountX, float amountY)
    {
        public ChargeType Type = type;
        public NPC NPC = npc;
        public Projectile Projectile = projectile;
        public float AmountX = amountX;
        public float AmountY = amountY;
    }
    public abstract class Favor : ModItem
    {
        public float Charge { get; set; } = 0f;
        private float prevCharge = 0f;
        private float chargeScale;
        private bool chargeScaleDirection = false;
        /// <summary>
        /// The amount of time Favor Fatigue should be applied for after usage. Return 0 for no Favor Fatigue.
        /// </summary>
        public abstract int FavorFatigueTime { get; }
        public override void SaveData(TagCompound tag)
        {
            tag.Add("favorItemCharge", Charge);
        }
        public override void LoadData(TagCompound tag)
        {
            Charge = tag.Get<float>("favorItemCharge");
        }
        public sealed override bool ConsumeItem(Player player) => false;
        public sealed override bool CanRightClick() => true;
        public sealed override void RightClick(Player player)
        {
            ref Item playerFavorItem = ref player.GetModPlayer<FavorPlayer>().FavorItem;
            if (playerFavorItem is null || playerFavorItem.IsAir)
            {
                playerFavorItem = Item.Clone();
                Item.TurnToAir();
            }
            else
            {
                Item temp = playerFavorItem.Clone();
                playerFavorItem = Item.Clone();
                Item.SetDefaults(temp.type);
            }
        }
        /// <summary>
        /// Allows you to decide how much charge should be added to the Favor.
        /// </summary>
        /// <param name="chargeData">Information about the type of charge received and other relevant parameters about the charge.</param>
        /// <returns></returns>
        public virtual float ChargeAmount(ChargeData chargeData)
        {
            return chargeData.AmountX / 20f;
        }
        public void ChargeFavor(float amount)
        {
            prevCharge = Charge;
            Charge = Math.Clamp(Charge + amount, 0f, 1f);
            if (Charge >= 1f && prevCharge != Charge) // just got charged to max
            {
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Items/Favors/ChargeSounds/" + GetChargeSound()));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// <para>True: Favor has been triggered. Apply Favor Fatigue and use up Charge.</para> 
        /// <para>False: Favor hasn't done anything. Don't apply Favor Fatigue and don't use Charge.</para
        /// </returns>
        public virtual bool UseFavor(Player player)
        {
            return true;
        }
        /*
        public sealed override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            return slot == ModContent.GetInstance<FavorSlot>().Type;
        }
        */
        public virtual void UpdateFavor(Player player, bool hideVisual)
        {

        }
        public sealed override void UpdateAccessory(Player player, bool hideVisual)
        {
            FavorPlayer favorPlayer = player.GetModPlayer<FavorPlayer>();
            favorPlayer.FavorItem = Item;
            UpdateFavor(player, hideVisual);
        }
        /// <summary>
        /// Override to set a custom BarStyle for the item being drawn in the inventory.
        /// </summary>
        /// <returns>The name of the bar style. The item will look for an overlay image that has the same path, but with _Overlay added to the end of the name.</returns>
        public virtual string GetBarStyle()
        {
            return "DefaultBarStyle";
        }
        public virtual string GetChargeSound()
        {
            return "DefaultChargeSound";
        }
        public virtual void PreDrawInInventoryAfterSheen(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {

        }
        public sealed override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Charge >= 1f)
            {
                chargeScaleDirection = true;
            }
            else
            {
                chargeScaleDirection = false;
            }
            chargeScale = Math.Clamp(chargeScale + (chargeScaleDirection ? 0.01f : -0.01f), 0f, 1f);
            if (chargeScale > 0f)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                Texture2D flare = ModContent.Request<Texture2D>("ITD/Content/UI/LensFlare").Value;
                float drawScale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) / 2f;
                float addDrawScale = drawScale / 8f;
                spriteBatch.Draw(flare, position, null, Color.White * drawScale * chargeScale, Main.GlobalTimeWrappedHourly, flare.Size() * 0.5f, 0.2f + addDrawScale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                PreDrawInInventoryAfterSheen(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            }
            return true;
        }
        public sealed override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            string barPath = "ITD/Content/Items/Favors/BarStyles/" + GetBarStyle();
            Texture2D barTexture = ModContent.Request<Texture2D>(barPath).Value;
            Texture2D barOverlay = ModContent.Request<Texture2D>(barPath + "_Overlay").Value;
            int barHeight = barTexture.Height;
            int barWidth = 42;
            int barSegment = barTexture.Width / 3;
            float centerSegmentXScale = (float)(barWidth - barSegment * 2f) / barSegment;
            float chargeWidth = barWidth * Charge;
            float centerSegmentChargeScale = (float)(chargeWidth - barSegment * 2f) / barSegment;
            centerSegmentChargeScale = Helpers.Remap(centerSegmentChargeScale, -2f, centerSegmentXScale, 0f, centerSegmentXScale );
            for (int i = 0; i < 3; i++)
            {
                Rectangle rect = new Rectangle(barSegment * i, 0, barSegment, barHeight);
                spriteBatch.Draw(
                    barTexture,
                    new Vector2(position.X - (barWidth / 2f) + barSegment * i + (i == 2 ? barSegment * centerSegmentXScale - barSegment : 0), position.Y + barHeight),
                    rect, Color.White, 0f, default,
                    new Vector2(i == 1 ? centerSegmentXScale : 1f, 1f),
                    SpriteEffects.None, 0f
                );
            }
            for (int i = 0; i < 3; i++)
            {
                Rectangle rect = new Rectangle(barSegment * i, 0, barSegment, barHeight);
                spriteBatch.Draw(
                    barOverlay,
                    new Vector2(position.X - (barWidth / 2f) + barSegment * i + (i == 2 ? barSegment * centerSegmentChargeScale - barSegment : 0), position.Y + barHeight),
                    rect, Color.White, 0f, default,
                    new Vector2(i == 1 ? centerSegmentChargeScale : 1f, 1f),
                    SpriteEffects.None, 0f
                );
            }
        }
    }
}
