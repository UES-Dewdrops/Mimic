using RoR2;
using MimicMod.Modules.Achievements;

namespace MimicMod.Survivors.Mimic.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class MimicMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = MimicSurvivor.Mimic_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = MimicSurvivor.Mimic_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => MimicSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}