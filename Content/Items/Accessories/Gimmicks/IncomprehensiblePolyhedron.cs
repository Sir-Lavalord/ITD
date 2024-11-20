using ITD.Utilities.Placeholders;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Gimmicks
{
    public class IncomprehensiblePolyhedron : ModItem
    {
        public override string Texture => Placeholder.PHBigGun;
        public override void SetDefaults()
        {
            Item.DefaultToAccessory();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<A3DPlayer>().Active = true;
        }
    }
    public class A3DPlayer : ModPlayer
    {
        public bool Active = false;
        public override void ResetEffects()
        {
            Active = false;
        }
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (!Active)
                return;
            foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
            {
                layer.Hide();
            }
        }
    }
}
