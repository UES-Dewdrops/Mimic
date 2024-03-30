using EntityStates;
using HG;
using MimicMod.Survivors.Mimic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MimicMod.SkillStates.Primary
{
    public class TarShotgun : BaseSkillState
    {
        public static float damageCoefficient = MimicStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 0.4f; // prev 1
        public static float baseDuration = 0.6f;
        public static float force = 100f; // prev: 800f
        public static float recoil = 6f;
        public static float range = 55f; // prev: 100f, 233, 256
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        public static float minSpread = 6f; // prev: 2f, 0.5f
        public static float maxSpread = 10f; // prev: 4f, 2f
        public static uint bulletCount = 6; // prev: 5, 7, 5, 6
        public static float spreadBloom = 1.5f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = TarShotgun.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";

            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.radius = 2; // prev 4
            sphereSearch.origin = characterBody.transform.position + this.inputBank.aimDirection;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            int numHurtBoxes = sphereSearch.GetHurtBoxes().Length;

            if (NetworkServer.active && numHurtBoxes > 0)
            {
                base.characterBody.AddTimedBuff(MimicBuffs.escapeBuff, 2f);
            }


            base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(spreadBloom);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-1f * TarShotgun.recoil, -2f * TarShotgun.recoil, -0.5f * TarShotgun.recoil, 0.5f * TarShotgun.recoil);

                    var bulletAttack = new BulletAttack
                    {
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = TarShotgun.damageCoefficient * this.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.ClayGoo,
                        falloffModel = BulletAttack.FalloffModel.Buckshot,
                        maxDistance = TarShotgun.range,
                        force = TarShotgun.force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 2f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = TarShotgun.tracerEffectPrefab,
                        spreadPitchScale = 0.7f, // prev: 1 0f
                        spreadYawScale = 0.7f, //prev 1 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                        HitEffectNormal = false
                    };

                    bulletAttack.minSpread = minSpread; // prev: 0
                    bulletAttack.maxSpread = maxSpread; //prev: 0
                    bulletAttack.bulletCount = 1;
                    bulletAttack.Fire();

                    uint secondShot = (uint)Mathf.CeilToInt(bulletCount / 2f) - 1;
                    bulletAttack.minSpread = minSpread; // prev: 0
                    bulletAttack.maxSpread = maxSpread * 1.45f; // prev: spread / 1.45
                    bulletAttack.bulletCount = secondShot;
                    bulletAttack.Fire();

                    bulletAttack.minSpread = minSpread * 1.45f; // prev: spread / 1.45
                    bulletAttack.maxSpread = maxSpread * 2f; // prev: spread
                    bulletAttack.bulletCount = (uint)Mathf.FloorToInt(bulletCount / 2f);
                    bulletAttack.Fire();

                    bulletAttack.minSpread = minSpread * 2f; // prev: spread / 1.45
                    bulletAttack.maxSpread = maxSpread * 4f; // prev: spread
                    bulletAttack.bulletCount = (uint)Mathf.FloorToInt(bulletCount / 2f);
                    bulletAttack.Fire();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}