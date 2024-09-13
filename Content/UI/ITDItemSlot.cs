using ITD.Content.Items.Favors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StructureHelper.Core.Loaders.UILoading;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.GameContent;
namespace ITD.Content.UI
{
    public abstract class ITDItemSlot : UIElement
    {
        public abstract ref Item item { get; }
        public abstract Func<Item, bool> isValid { get; }
        public abstract string Texture { get; }
        public bool NoItem => item is null || item.IsAir;
        public void UpdateProperties(float dimension, float left, float top)
        {
            Width.Set(dimension, 0f);
            Height.Set(dimension, 0f);
            Left.Set(left, 0f);
            Top.Set(top, 0f);
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            if (!Main.playerInventory)
                return;
            if (!Main.mouseItem.IsAir && isValid(Main.mouseItem) && NoItem)
            {
                item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (Main.mouseItem.IsAir && !NoItem)
            {
                Main.mouseItem = item.Clone();
                item.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (!Main.mouseItem.IsAir && isValid(Main.mouseItem) && !NoItem)
            {
                var temp = item.Clone();
                item = Main.mouseItem.Clone();
                Main.mouseItem = temp;
            }
            base.LeftClick(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (!NoItem)
            {
                if (item.ModItem is Favor favorItem)
                {
                    favorItem.UpdateAccessory(Main.LocalPlayer, false);
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), Color.White);
            if (!NoItem)
            {
                ItemSlot.DrawItemIcon(item, 21, spriteBatch, GetDimensions().Position() + new Vector2(GetDimensions().Width / 2, GetDimensions().Height / 2), 1f, 64f, Color.White);
                if (IsMouseHovering)
                {
                    Main.HoverItem = item.Clone();
                }
            }
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
    }
}