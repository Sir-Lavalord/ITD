using ITD.Content.Rarities;
using ITD.Content.UI;
using ITD.Systems;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Utilities;
using ReLogic.Utilities;
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
        /// <summary>
        /// If true, the Charge bar will not be drawn. Internally, the item will always be at 100% Charge.
        /// </summary>
        public abstract bool IsCursedFavor { get; }
        public override void SaveData(TagCompound tag)
        {
            tag.Add("favorItemCharge", Charge);
        }
        public override void LoadData(TagCompound tag)
        {
            Charge = tag.Get<float>("favorItemCharge");
        }
        /// <summary>
        /// Same function as SetStaticDefaults without extra stuff. You don't need to set the rarity here.
        /// </summary>
        public virtual void SetFavorDefaults()
        {

        }
        public sealed override void SetDefaults()
        {
            Item.rare = IsCursedFavor? ModContent.RarityType<CursedFavorRarity>() : ModContent.RarityType<FavorRarity>() ;
            SetFavorDefaults();
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
            return 0f;
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
        /// <summary>
        /// Allows you to do something while the Favor is in the Favor slot.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hideVisual"></param>
        public virtual void UpdateFavor(Player player, bool hideVisual)
        {

        }
        public sealed override void UpdateAccessory(Player player, bool hideVisual)
        {
            FavorPlayer favorPlayer = player.GetModPlayer<FavorPlayer>();
            favorPlayer.FavorItem = Item;
            if (IsCursedFavor)
                Charge = 1f;
            UpdateFavor(player, hideVisual);
        }
        /// <summary>
        /// Override to do things when this favor is unequipped, like resetting fields.
        /// </summary>
        public virtual void OnUnequip()
        {

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
                float drawScale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 5f) / 6f;
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
            if (IsCursedFavor)
                return;

            string barPath = "ITD/Content/Items/Favors/BarStyles/" + GetBarStyle();
            Texture2D barTexture = ModContent.Request<Texture2D>(barPath).Value;

            int barHeight = barTexture.Height;
            int barWidth = barTexture.Width;
 
            float chargeWidth = barWidth / 2 * Charge;
            Vector2 drawPos = new(position.X - (barWidth / 2 / 2), position.Y + barHeight);

            Rectangle barRect = barTexture.Frame(2, 1, 0);
            Rectangle overlayRect = new(barWidth / 2, 0, (int)chargeWidth, barHeight);

            spriteBatch.Draw(barTexture, drawPos, barRect, Color.White, 0f, default, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(barTexture, drawPos, overlayRect, Color.White, 0f, default, 1f, SpriteEffects.None, 0f);
        }
    }
}
