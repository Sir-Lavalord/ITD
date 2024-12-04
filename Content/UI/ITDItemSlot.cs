using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
namespace ITD.Content.UI
{
    public abstract class ITDItemSlot : ITDUIElement
    {
        public abstract ref Item item { get; }
        public abstract Func<Item, bool> isValid { get; }
        public abstract string Texture { get; }
        public bool NoItem => item is null || item.IsAir;
        public bool IsMouseOver => ContainsPoint(Main.MouseScreen);
        public void UpdateProperties(float dimension, float left, float top)
        {
            Width.Set(dimension, 0f);
            Height.Set(dimension, 0f);
            Left.Set(left, 0f);
            Top.Set(top, 0f);
        }
        public virtual void PostClickItemIn(ref Item slotItem)
        {

        }
        public virtual void PostClickItemOut(ref Item mouseItem)
        {

        }
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
        public sealed override void LeftClick(UIMouseEvent evt)
        {
            if (!Main.playerInventory)
                return;
            if (!Main.mouseItem.IsAir && isValid(Main.mouseItem) && NoItem)
            {
                item = Main.mouseItem.Clone();
                PostClickItemIn(ref item);
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (Main.mouseItem.IsAir && !NoItem)
            {
                Main.mouseItem = item.Clone();
                PostClickItemOut(ref Main.mouseItem);
                item.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (!Main.mouseItem.IsAir && isValid(Main.mouseItem) && !NoItem)
            {
                var temp = item.Clone();
                item = Main.mouseItem.Clone();
                Main.mouseItem = temp;
                PostClickItemIn(ref item);
                PostClickItemOut(ref Main.mouseItem);
                SoundEngine.PlaySound(SoundID.Grab);
            }
            base.LeftClick(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseOver)
                Main.LocalPlayer.mouseInterface = true;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), Color.White);
            if (!NoItem)
            {
                ItemSlot.DrawItemIcon(item, 21, spriteBatch, GetDimensions().Position() + new Vector2(GetDimensions().Width / 2, GetDimensions().Height / 2), 1f, 64f, Color.White);
                if (IsMouseOver)
                {
                    Main.HoverItem = item.Clone();
                    Main.hoverItemName = "a";
                }
            }
            if (IsMouseOver)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            PostDraw(spriteBatch);
        }
    }
}