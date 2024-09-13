using ITD.Content.Items.Favors;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using ITD.Systems;
using System.Collections.Generic;
using ReLogic.Graphics;

namespace ITD.Content.UI
{
    public class FavorSlotGui : ITDUIState
    {
        private FavorSlot favor;
        public override bool Visible => true;
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
        }
        public override void OnInitialize()
        {
            favor = new FavorSlot();

            if (Main.playerInventory)
            {
                favor.UpdateProperties(52f, Main.screenWidth - 500, 30);
            }
            else
            {
                favor.UpdateProperties(52f, Main.screenWidth - 370, 30);
            }

            Append(favor);
        }
        public override void Update(GameTime gameTime)
        {
            // this is really stupid but trying to change the properties directly here doesn't work for some reason
            RemoveAllChildren();
            OnInitialize();
            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
    public class FavorSlot : ITDItemSlot
    {
        public override ref Item item => ref Main.LocalPlayer.GetModPlayer<FavorPlayer>().FavorItem;
        public override Func<Item, bool> isValid => (item) => item.ModItem is Favor;
        public override string Texture => "ITD/Content/UI/FavorSlot";
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!NoItem)
            {
                if (item.ModItem is Favor favorItem)
                {
                    favorItem.UpdateAccessory(Main.LocalPlayer, false);
                }
            }
        }
    }
}
