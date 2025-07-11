using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
namespace ITD.Content.Items.Accessories.Master
{
    public class EatersTail : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<EoWTailPlayer>().eowMasterAcc = true;
        }
    }
    public class EoWTailPlayer : ModPlayer
    {
        public int revtime;//Revival time
        public bool eowFirstBar = true;//Check if is first healthbar
        public bool tailregained;//Have tail or not
        public int eowLifeCap;//Life cap
        public int testnum;//Equip/Unequip
        public bool eowMasterAcc;
        string RevText;

        public override void UpdateDead()
        {
            revtime = 0;
            eowFirstBar = true;
            eowLifeCap = Player.statLifeMax2 / 2;
            testnum = 0;
        }
        public override void ResetEffects() //Resets bools if the item is unequipped
        {
            eowMasterAcc = false;
        }
        public override void PostUpdateEquips() //Updates every frame
        {
            if (eowMasterAcc)
            {
                eowLifeCap = Player.statLifeMax2 / 2;
                if (revtime >= 0 && Player.statLife == eowLifeCap && !eowFirstBar)
                {
                    if (Main.rand.NextBool(8))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int dustIndex = Dust.NewDust(new Vector2(Player.position.X, Player.position.Y), Player.width, Player.height, DustID.HealingPlus, 0, 0, 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                            Dust dust = Main.dust[dustIndex];
                            dust.noGravity = true;
                        }
                    }
                    revtime--;
                }
                if (revtime <= 0)
                {
                    eowFirstBar = true;
                }
                Player.statLifeMax2 = Player.statLifeMax2 / 2;
            }
            if (revtime > 0)//not getting away with it
            {
                Player.statLifeMax2 = eowLifeCap;
            }
            //testing
            if (eowFirstBar && eowMasterAcc)
            {
                if (testnum < 10)//graphicamajig
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath1, Player.Center);
                    testnum++;
                }
            }
            else
            {
                if (testnum > 0)//graphicamajig
                {
                    testnum--;
                }
            }
        }
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (eowMasterAcc)
            {
                if (eowFirstBar && damage <= 10000)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        int dustIndex = Dust.NewDust(new Vector2(Player.position.X, Player.position.Y), Player.width, Player.height, DustID.CorruptGibs, 0, 0, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                        Dust dust = Main.dust[dustIndex];
                        dust.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Roar, Player.Center);
                    SoundEngine.PlaySound(SoundID.NPCDeath1, Player.Center);


                    var entitySource = Player.GetSource_FromThis();

                    for (int i = 0; i < 1; i++)
                    {
                        Gore.NewGore(entitySource, Player.Center, new Vector2(Main.rand.Next(-2, 2)), 27);
                        Gore.NewGore(entitySource, Player.Center, new Vector2(Main.rand.Next(-2, 1)), 28);
                        Gore.NewGore(entitySource, Player.Center, new Vector2(Main.rand.Next(-1, 1)), 29);
                    }
                    Player.HealEffect(eowLifeCap, true);
                    Player.statLife = eowLifeCap;
                    Player.immune = true;
                    Player.immuneTime = 180;
                    eowFirstBar = false;
                    tailregained = false;
                    revtime = 3600;
                    string Lost = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(EatersTail)}.TailLost")).Value;
                    RevText = Lost;
                    CombatText.NewText(new Rectangle((int)Player.Center.X, (int)Player.Center.Y, 12, 4), Color.Red, RevText, true);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
