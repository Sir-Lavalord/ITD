using ITD.Content.Items.PetSummons;

namespace ITD.Utilities.Placeholders
{
    internal class Placeholder
    {
        /* In case there is content without a sprite put following into the code:
         
            using ITD.Utilities.Placeholders;
            ...
            public override string Texture => Placeholder.PHSword;

        otherwise you can use:

            public override string Texture => Utilities.Placeholders.Placeholder.PHSword;

        -Use the texture that is neccessary for your content(see strings bellow)
        -For Armor you just need to override the texture of the item, the texture when eqiped changes automatically
        -Also you dont need to add an image to your stuff as there wont be a crash without it

        I'll try to add some more placeholders later if this gets used
        */

        public const string PHGeneric = "ITD/Utilities/Placeholders/PHGeneric";
        //Weapons
        public const string PHSword = "ITD/Utilities/Placeholders/PHSword";
        public const string PHSpear = "ITD/Utilities/Placeholders/PHSpear";
        public const string PHYoyo = "ITD/Utilities/Placeholders/PHYoyo";
        public const string PHBoomerang = "ITD/Utilities/Placeholders/PHBoomerang";
        public const string PHFlail = "ITD/Utilities/Placeholders/PHFlail";

        public const string PHBow = "ITD/Utilities/Placeholders/PHBow";
        public const string PHRepeater = "ITD/Utilities/Placeholders/PHRepeater";
        public const string PHGun = "ITD/Utilities/Placeholders/PHGun";
        public const string PHBigGun = "ITD/Utilities/Placeholders/PHBigGun";
        public const string PHBlowpipe = "ITD/Utilities/Placeholders/PHBlowpipe";
        public const string PHTrowable = "ITD/Utilities/Placeholders/PHTrowable";

        public const string PHStaff = "ITD/Utilities/Placeholders/PHStaff";
        public const string PHBook = "ITD/Utilities/Placeholders/PHBook";

        public const string PHWhip = "ITD/Utilities/Placeholders/PHWhip";

        //Tools
        public const string PHPickaxe = "ITD/Utilities/Placeholders/PHPickaxe";
        public const string PHAxe = "ITD/Utilities/Placeholders/PHAxe";
        public const string PHHammer = "ITD/Utilities/Placeholders/PHHammer";

        //Armor
        public const string PHHelmet = "ITD/Utilities/Placeholders/PHHelmet";
        public const string PHBreastplate = "ITD/Utilities/Placeholders/PHBreastplate";
        public const string PHLeggings = "ITD/Utilities/Placeholders/PHLeggings";
        public const string PHHelmetHead = "ITD/Utilities/Placeholders/PHHelmet_Head";
        public const string PHBreastplateBody = "ITD/Utilities/Placeholders/PHBreastplate_Body";
        public const string PHLeggingsLegs = "ITD/Utilities/Placeholders/PHLeggings_Legs";

        //Buffs
        public const string PHBuff = "ITD/Utilities/Placeholders/PHBuff";
        public const string PHDebuff = "ITD/Utilities/Placeholders/PHDebuff";

        //Accessories
        public const string PHBottle = "ITD/Utilities/Placeholders/PHBottle";
    }
}
