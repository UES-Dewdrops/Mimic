using EntityStates;
using MimicMod.Survivors.Mimic;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MimicMod.SkillStates.Utility
{
    public class Roll : BaseSkillState
    {
        public static float duration = 2.5f; // 0.5
        public static float initialSpeedCoefficient = 1.25f; // prev: 4 2.5
        public static float finalSpeedCoefficient = 2.0f; // prev: 2 2
        public static float rollYOffset = 0.75f; //prev: 0.35 1.25 0.25 0.55f

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;
        public static GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();

        private Vector3 rollDirection;
        private readonly float minimumY = 6; // 6, 4
        private readonly float aimVelocity = 3; // 4, 2
        private readonly float forwardVelocity = 30; // prev: 4, 6, 3
        private readonly float upwardVelocity = 7f; // prev: 8, 10 5

        public static float baseRadius = 3f;
        public static float baseForce = 10f;
        public static float dmgMod = 5f; // prev: 10

        private float escapeBoost = 1f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            if (NetworkServer.active && base.characterBody.HasBuff(MimicBuffs.escapeBuff)) {
                escapeBoost = 3f;
            } else {
                escapeBoost = 1f;
            }

            Vector3 footPosition = base.characterBody.footPosition;
            EffectManager.SpawnEffect(impactEffect, new EffectData
            {
                origin = footPosition,
                scale = 1
            }, transmit: true);

            new BlastAttack
            {
                attacker = base.gameObject,
                baseDamage = damageStat * (dmgMod + escapeBoost),
                baseForce = baseForce,
                bonusForce = Vector3.back, // up
                crit = false, //isCritAuthority,
                damageType = DamageType.ClayGoo,
                falloffModel = BlastAttack.FalloffModel.Linear, // None
                procCoefficient = 0.1f, // 0.5
                radius = baseRadius + escapeBoost, 
                position = base.characterBody.footPosition,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                impactEffect = EffectCatalog.FindEffectIndexFromPrefab(impactEffect),
                teamIndex = base.teamComponent.teamIndex,
            }.Fire();

            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = false;
                direction.y = Mathf.Max(direction.y, (minimumY + escapeBoost));
                Vector3 val = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 val2 = Vector3.up * (upwardVelocity + escapeBoost);
                Vector3 val3 = new Vector3(direction.x, 0f, direction.z);
                Vector3 val4 = val3.normalized * forwardVelocity;
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = val + val2 + val4;
            }

            if (NetworkServer.active)
            {
                base.characterBody.armor += 25f;
                base.characterBody.baseArmor += 25f;
            }

            base.characterDirection.moveVector = direction;

            base.PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", Roll.duration);
            Util.PlaySound(Roll.dodgeSoundString, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Roll.dodgeFOV, 60f, base.fixedAge / Roll.duration);
            base.characterMotor.moveDirection = base.inputBank.moveVector;

            if (base.isAuthority && base.characterMotor.isGrounded)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
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