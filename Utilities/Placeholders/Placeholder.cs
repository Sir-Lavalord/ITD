namespace ITD.Utilities.Placeholders
{
    internal class Placeholder
    {
        /* In case there is content without a sprite put following into the code:
         
            using ITD.Utils.Placeholders;
            ...
            public override string Texture => Placeholder.PHSword;

        otherwise you can use:

            public override string Texture => Utils.Placeholders.Placeholder.PHSword;

        -Use the texture that is neccessary for your content(see strings bellow)
        -For Armor you just need to override the texture of the item, the texture when eqiped changes automatically
        -Also you dont need to add an image to your stuff as there wont be a crash without it

        I'll try to add some more placeholders later if this gets used
        */

        public const string PHGeneric = "ITD/Utils/Placeholders/PHGeneric";
        //Weapons
        public const string PHSword = "ITD/Utils/Placeholders/PHSword";
        public const string PHSpear = "ITD/Utils/Placeholders/PHSpear";
        public const string PHYoyo = "ITD/Utils/Placeholders/PHYoyo";
        public const string PHBoomerang = "ITD/Utils/Placeholders/PHBoomerang";
        public const string PHFlail = "ITD/Utils/Placeholders/PHFlail";

        public const string PHBow = "ITD/Utils/Placeholders/PHBow";
        public const string PHRepeater = "ITD/Utils/Placeholders/PHRepeater";
        public const string PHGun = "ITD/Utils/Placeholders/PHGun";
        public const string PHBigGun = "ITD/Utils/Placeholders/PHBigGun";
        public const string PHBlowpipe = "ITD/Utils/Placeholders/PHBlowpipe";
        public const string PHTrowable = "ITD/Utils/Placeholders/PHTrowable";

        public const string PHStaff = "ITD/Utils/Placeholders/PHStaff";
        public const string PHBook = "ITD/Utils/Placeholders/PHBook";

        public const string PHWhip = "ITD/Utils/Placeholders/PHWhip";

        //Tools
        public const string PHPickaxe = "ITD/Utils/Placeholders/PHPickaxe";
        public const string PHAxe = "ITD/Utils/Placeholders/PHAxe";
        public const string PHHammer = "ITD/Utils/Placeholders/PHHammer";

        //Armor
        public const string PHHelmet = "ITD/Utils/Placeholders/PHHelmet";
        public const string PHBreastplate = "ITD/Utils/Placeholders/PHBreastplate";
        public const string PHLeggings = "ITD/Utils/Placeholders/PHLeggings";
        public const string PHHelmetHead = "ITD/Utils/Placeholders/PHHelmet_Head";
        public const string PHBreastplateBody = "ITD/Utils/Placeholders/PHBreastplate_Body";
        public const string PHLeggingsLegs = "ITD/Utils/Placeholders/PHLeggings_Legs";

        //Buffs
        public const string PHBuff = "ITD/Utils/Placeholders/PHBuff";
        public const string PHDebuff = "ITD/Utils/Placeholders/PHDebuff";

        //Mounts
        public const string PHMount = "ITD/Utils/Placeholders/PHMount";

        //NPCs
        public const string PHPerson = "ITD/Utils/Placeholders/PHPerson";
    }
}
