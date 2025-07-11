namespace ITD.Content.Buffs.FavorBuffs
{
    public class Trailblazing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
			player.moveSpeed += 0.005f * player.buffTime[buffIndex];
			player.DoBootsEffect(new Utils.TileActionAttempt(player.DoBootsEffect_PlaceFlamesOnTile));
        }
    }
}
