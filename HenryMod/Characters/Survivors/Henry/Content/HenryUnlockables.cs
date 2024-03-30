using MimicMod.Survivors.Mimic.Achievements;
using RoR2;
using UnityEngine;

namespace MimicMod.Survivors.Mimic
{
    public static class MimicUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                MimicMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(MimicMasteryAchievement.identifier),
                MimicSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
