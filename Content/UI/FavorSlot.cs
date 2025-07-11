using ITD.Content.Items.Favors;
using System;
using Terraria.UI;
using ITD.Systems;
using System.Collections.Generic;
using Terraria.ModLoader.UI;

namespace ITD.Content.UI
{
    public class FavorSlotGui : ITDUIState
    {
        private FavorSlot favor;
        public override bool Visible => Main.LocalPlayer.TryGetModPlayer(out FavorPlayer favorPlayer) && favorPlayer.FavorSlotVisible;
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
        }
        public override void OnInitialize()
        {
            favor = new FavorSlot();
            Append(favor);
        }
        public override void Update(GameTime gameTime)
        {
            bool mapIconsShown = Main.screenWidth >= 940 && Main.playerInventory; // same check as vanilla

            favor.UpdateProperties(52f, Main.screenWidth - (mapIconsShown ? 500 : 370), 30);

            Recalculate();
            base.Update(gameTime);
        }
    }
    public class FavorSlot : ITDItemSlot
    {
        public override ref Item item => ref Main.LocalPlayer.GetModPlayer<FavorPlayer>().FavorItem;
        public override Func<Item, bool> isValid => (item) => item.ModItem is Favor;
        public override string Texture => "ITD/Content/UI/FavorSlot";
        public override void Update(GameTime gameTime)
        {
            if (IsMouseOver)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (NoItem)
                    UICommon.TooltipMouseText(this.GetLocalization("MouseHoverName").Value);
            }
        }
        public override void PostClickItemOut(ref Item mouseItem)
        {
            if (mouseItem.ModItem is Favor favorItem)
            {
                favorItem.Charge = 0f;
                favorItem.OnUnequip();
            }
        }
        public override void PostClickItemIn(ref Item slotItem)
        {
            if (slotItem.ModItem is Favor favorItem)
            {
                favorItem.Charge = 0f;
                favorItem.OnUnequip();
            }
        }
    }
}
