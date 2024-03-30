using RoR2;
using UnityEngine;

namespace MimicMod.Survivors.Mimic
{
    public static class MimicBuffs
    {
        // armor buff gained during roll
        public static BuffDef armorBuff;

        public static BuffDef bombBuff;

        public static BuffDef escapeBuff;

        public static void Init(AssetBundle assetBundle)
        {
            armorBuff = Modules.Content.CreateAndAddBuff("MimicArmorBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.white,
                false,
                false);

            bombBuff = Modules.Content.CreateAndAddBuff("TarMimicBombBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.black,
                true,
                false);

            escapeBuff = Modules.Content.CreateAndAddBuff("TarMimicEscapeBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.red,
                true,
                false);

        }
    }
}
