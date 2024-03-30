using System;
using MimicMod.Modules;
using MimicMod.Survivors.Mimic.Achievements;

namespace MimicMod.Survivors.Mimic
{
    public static class MimicTokens
    {
        public static void Init()
        {
            AddMimicTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Mimic.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddMimicTokens()
        {
            string prefix = MimicSurvivor.Mimic_PREFIX;

            string desc = "Mimic is a being reborn by tar with a powerful and mobile close ranged kit." + Environment.NewLine + Environment.NewLine
             + "< ! > Mimic's Blunderbuss is a powerful close ranged slugger that tars enemies. Firing when enemies are nearby grants additional damage and an escape buff." + Environment.NewLine + Environment.NewLine
             + "< ! > Bombs are weak on their own, but ignite enemies. Using bombs with stacks of bomb buffs consumes them and increases damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Launch allows mimic to quickly escape fights, damaging enemies as they reposition. Using launch with an escape buff increases vertical leap." + Environment.NewLine + Environment.NewLine
             + "< ! > Chest Slam is a powerful crowd control attack, allowing mimic to wipe out weak foes, and stun tougher ones. Hitting enemies grants stackable bomb buffs." + Environment.NewLine + Environment.NewLine;


            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", "Mimic");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Chosen One");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Mimic passive");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Sample text.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SLASH_NAME", "Sword");
            Language.Add(prefix + "PRIMARY_SLASH_DESCRIPTION", Tokens.agilePrefix + $"Swing forward for <style=cIsDamage>{100f * MimicStaticValues.swordDamageCoefficient}% damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_GUN_NAME", "Handgun");
            Language.Add(prefix + "SECONDARY_GUN_DESCRIPTION", Tokens.agilePrefix + $"Fire a handgun for <style=cIsDamage>{100f * MimicStaticValues.gunDamageCoefficient}% damage</style>.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_ROLL_NAME", "Roll");
            Language.Add(prefix + "UTILITY_ROLL_DESCRIPTION", "Roll a short distance, gaining <style=cIsUtility>300 armor</style>. <style=cIsUtility>You cannot be hit during the roll.</style>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BOMB_NAME", "Bomb");
            Language.Add(prefix + "SPECIAL_BOMB_DESCRIPTION", $"Throw a bomb for <style=cIsDamage>{100f * MimicStaticValues.bombDamageCoefficient}% damage</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(MimicMasteryAchievement.identifier), "Mimic: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(MimicMasteryAchievement.identifier), "As Mimic, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
