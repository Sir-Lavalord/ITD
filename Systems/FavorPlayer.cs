using ITD.Content.Buffs.Debuffs;
using ITD.Content.Items.Favors;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace ITD.Systems;

public class FavorPlayer : ModPlayer
{
    public Item FavorItem;
    public bool favorFatigue;
    public static ModKeybind UseFavorKey { get; private set; } = null;
    public static string FavorKeybindString { get { return UseFavorKey.GetAssignedKeys().FirstOrDefault("[Unbound Key]"); } }
    public bool FavorSlotVisible { get { return (!FavorItem.IsAir && FavorItem != null) || Player.inventory.Any(i => i.ModItem is Favor); } }
    public override void Load()
    {
        UseFavorKey = KeybindLoader.RegisterKeybind(Mod, "UseFavor", Keys.F);
    }
    public override void Initialize()
    {
        FavorItem = new Item();
    }
    public override void SaveData(TagCompound tag)
    {
        tag.Add("favorItem", FavorItem);
    }
    public override void LoadData(TagCompound tag)
    {
        FavorItem = tag.Get<Item>("favorItem");
    }
    public override void ResetEffects()
    {
        favorFatigue = false;
    }
    public void UseFavor()
    {
        if (FavorItem.ModItem is Favor favorItem)
        {
            if (favorItem.UseFavor(Player))
            {
                if (!favorItem.IsCursedFavor)
                    favorItem.Charge = 0f;
                if (favorItem.FavorFatigueTime > 0)
                    Player.AddBuff<FavorFatigue>(favorItem.FavorFatigueTime);
            }
        }
    }
    private bool TryUseFavor()
    {
        return Player.IsLocalPlayer() && FavorItem != null && FavorItem.ModItem is Favor favorItem && favorItem.Charge >= 1f && UseFavorKey.JustPressed && !favorFatigue;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (FavorItem != null && FavorItem.ModItem is Favor favorItem)
        {
            favorItem.ChargeFavor(favorItem.ChargeAmount(new ChargeData(ChargeType.DamageGiven, target, null, damageDone, 0f)));
            if (target.life <= 0)
                favorItem.ChargeFavor(favorItem.ChargeAmount(new ChargeData(ChargeType.EnemiesKilled, target, null, 1f, 0f)));
        }
    }
    public override void OnHurt(Player.HurtInfo info)
    {
        info.DamageSource.TryGetCausingEntity(out Entity entity);
        Projectile proj = null;
        NPC nPC = null;
        if (entity is Projectile projectile)
        {
            proj = projectile;
        }
        else if (entity is NPC npc)
        {
            nPC = npc;
        }
        if (FavorItem != null && FavorItem.ModItem is Favor favorItem)
        {
            favorItem.ChargeFavor(favorItem.ChargeAmount(new ChargeData(ChargeType.DamageTaken, nPC, proj, info.Damage, 0f)));
        }
    }
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (TryUseFavor())
        {
            UseFavor();
        }
    }
    public override void PostUpdate()
    {
        Vector2 distanceTraveled = Player.position - Player.oldPosition;
        if (distanceTraveled != Vector2.Zero && FavorItem != null && FavorItem.ModItem is Favor favorItem)
        {
            favorItem.ChargeFavor(favorItem.ChargeAmount(new ChargeData(ChargeType.DistanceTravelled, null, null, distanceTraveled.X, distanceTraveled.Y)));
        }
    }
    public override void UpdateEquips()
    {
        if (FavorItem != null && FavorItem.ModItem is Favor favorItem)
            favorItem.UpdateAccessory(Player, false);
    }
}