using EntityStates;
using R2API.Networking.Interfaces;
using R2API.Networking;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using MimicMod;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.BullseyeSearch;
using MimicMod.Modules;
using MimicMod.Survivors.Mimic;
using MimicMod.SkillStates.Utility;

namespace MimicMod.SkillStates.Special
{
    public class ChestSap : BaseSkillState
    {
        public static float duration = MimicStaticValues.chestSapLength;
        public static float initialSpeedCoefficient = 2.0f; // prev: 1.25 4 2.5
        public static float finalSpeedCoefficient = 6.0f; // prev: 2  2 2
        public static float rollYOffset = -1.25f; //prev: 1.25 0.25 0.55f
        public static float baseRadius = 5f;
        public static float baseForce = 25f;
        public static float dmgMod = 20f;
        

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public static GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();
        private static BuffDef chestBuff = RoR2Content.Buffs.Immune;

        private float rollSpeed;
        private Animator animator;
        private Vector3 rollDirection;
        private Vector3 previousPosition;
        private float slamAcceleration = 1f;
        private readonly int minimumY = 4;
        private readonly float upwardVelocity = 5f;
        private readonly float escapeBoost = 1f;

        public Vector3 handPosition;

        private List<TarTetherController> tetherControllers = new List<TarTetherController>();

        public List<HurtBox> affectedIndividuals = new List<HurtBox>();

        public static float maxTetherDistance = 60f;

        public static float mulchDistance = 5f;

        public static float mulchDamageScale = 2f;

        public static float mulchTickFrequencyScale = 0.5f;

        public static float damageTickFrequency = 3f;

        public static float damagePerSecond = 1f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            handPosition = base.characterBody.footPosition;

            // Have Mimic move slightly upward before slamming down
            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = false;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 upMotion = Vector3.up * (upwardVelocity + escapeBoost);
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = upMotion;
            }

            base.characterDirection.moveVector = direction;

            // ---- End move up ----

            if (base.isAuthority && base.inputBank && base.characterDirection)
            {
                //this.rollDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
                this.rollDirection = new Vector3(0, ((Vector3.down.y / 2) + rollYOffset), 0);
                
            }

            this.RecalculateRollSpeed();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = this.rollDirection * this.rollSpeed;
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            base.PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", Roll.duration);
            Util.PlaySound(Roll.dodgeSoundString, base.gameObject);

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(chestBuff);
            }
        }

        private void RecalculateRollSpeed()
        {
            this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(Roll.initialSpeedCoefficient, Roll.finalSpeedCoefficient, base.fixedAge / Roll.duration) + slamAcceleration;
        }

        private void FireTethers()
        {
            Vector3 val = handPosition;
            float breakDistanceSqr = maxTetherDistance * maxTetherDistance;
            List<GameObject> list = new List<GameObject>();
            BullseyeSearch val2 = new BullseyeSearch();
            val2.searchOrigin = val;
            val2.maxDistanceFilter = maxTetherDistance;
            val2.teamMaskFilter = TeamMask.allButNeutral;
            val2.sortMode = (SortMode)1;
            val2.filterByLoS = true;
            val2.searchDirection = Vector3.up;
            val2.RefreshCandidates();
            val2.FilterOutGameObject(this.gameObject);
            List<HurtBox> list2 = val2.GetResults().ToList();
            for (int i = 0; i < list2.Count; i++)
            {
                if (list.Count + tetherControllers.Count >= 20)
                {
                    break;
                }
                if (list2[i].healthComponent.body.teamComponent.teamIndex != this.characterBody.teamComponent.teamIndex && !affectedIndividuals.Contains(list2[i]))
                {
                    GameObject gameObject = list2[i].healthComponent.gameObject;
                    if (gameObject)
                    {
                        list.Add(gameObject);
                        affectedIndividuals.Add(list2[i]);
                    }
                }
            }
            float tickInterval = 1f / damageTickFrequency * (1f / this.characterBody.attackSpeed);
            float damageCoefficientPerTick = damagePerSecond / damageTickFrequency;
            float mulchDistanceSqr = mulchDistance * mulchDistance;
            GameObject val3 = Resources.Load<GameObject>("Prefabs/NetworkedObjects/TarTether");
            for (int j = 0; j < list.Count; j++)
            {
                GameObject val4 = UnityEngine.Object.Instantiate<GameObject>(val3, val, Quaternion.identity);
                TarTetherController component = val4.GetComponent<TarTetherController>();
                component.NetworkownerRoot = this.gameObject;
                component.NetworktargetRoot = list[j];
                component.breakDistanceSqr = breakDistanceSqr;
                component.damageCoefficientPerTick = damageCoefficientPerTick;
                component.tickInterval = tickInterval;
                component.tickTimer = (float)j * 0.1f;
                component.mulchDistanceSqr = mulchDistanceSqr;
                component.mulchDamageScale = mulchDamageScale;
                component.mulchTickIntervalScale = mulchTickFrequencyScale;
                component.reelSpeed = 0f;
                tetherControllers.Add(component);
                NetworkServer.Spawn(val4);
            }
        }

        private static void RemoveDeadTethersFromList(List<TarTetherController> tethersList)
        {
            for (int num = tethersList.Count - 1; num >= 0; num--)
            {
                if (!tethersList[num])
                {
                    tethersList.RemoveAt(num);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateRollSpeed();
            handPosition = base.characterBody.footPosition;
            slamAcceleration++; // We want to increase the speed of the slam as you slam down

            if (base.characterDirection) base.characterDirection.forward = this.rollDirection;
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Roll.dodgeFOV, 60f, base.fixedAge / Roll.duration);

            // Code for slamming down
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.rollSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.rollDirection), 0f);
                vector = this.rollDirection * d;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;

            // Code for controlling tethers
            for (int num2 = tetherControllers.Count - 1; num2 >= 0; num2--)
            {
                if (tetherControllers[num2])
                {
                    tetherControllers[num2].gameObject.transform.position = handPosition;
                    NetMessageExtensions.Send(new SyncTetherPosition(((NetworkBehaviour)tetherControllers[num2]).netId, new Vector3(handPosition.x, NetworkServer.localClientActive ? handPosition.y : (handPosition.y + 2f), handPosition.z)), (NetworkDestination)1);
                }
            }
            RemoveDeadTethersFromList(tetherControllers);
            FireTethers();

            if (base.isAuthority  && this.fixedAge > duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.characterBody.RemoveBuff(chestBuff);

            if (NetworkServer.active)
            {
                for (int num = tetherControllers.Count - 1; num >= 0; num--)
                {
                    if (tetherControllers[num])
                    {
                        UnityEngine.Object.Destroy(tetherControllers[num].gameObject);
                    }
                }
            }

            base.OnExit();

            base.characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.rollDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.rollDirection = reader.ReadVector3();
        }
    }
}