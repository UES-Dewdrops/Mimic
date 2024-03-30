
using MimicMod.SkillStates.Primary;
using MimicMod.SkillStates.Secondary;
using MimicMod.SkillStates.Special;
using MimicMod.SkillStates.Utility;
using UnityEngine.Networking;
using UnityEngine;
using R2API.Networking.Interfaces;

namespace MimicMod.Survivors.Mimic
{
    public class SyncTetherPosition : INetMessage, ISerializableObject
    {
        public NetworkInstanceId netIdToUpdate;

        public Vector3 positionSetter;

        public SyncTetherPosition()
        {
        }

        public SyncTetherPosition(NetworkInstanceId netID, Vector3 positionGiven)
        {
            netIdToUpdate = netID;
            positionSetter = positionGiven;
        }

        public void Deserialize(NetworkReader reader)
        {
            netIdToUpdate = reader.ReadNetworkId();
            positionSetter = reader.ReadVector3();
        }

        public void OnReceived()
        {
            if (!NetworkServer.active)
            {
                ClientScene.FindLocalObject(netIdToUpdate).transform.position = positionSetter;
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netIdToUpdate);
            writer.Write(positionSetter);
        }
    }

    public static class MimicStates
    {
        public static void Init()
        {
            // Primary
            Modules.Content.AddEntityState(typeof(TarShotgun));

            // Secondary
            Modules.Content.AddEntityState(typeof(ThrowFireBomb));
            Modules.Content.AddEntityState(typeof(ThrowTarBomb));

            // Utility
            Modules.Content.AddEntityState(typeof(Roll));

            // Special
            Modules.Content.AddEntityState(typeof(ChestRetreat));
            Modules.Content.AddEntityState(typeof(ChestSap));
        }
    }
}
