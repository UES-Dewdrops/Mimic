using EntityStates;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.ClayGrenadier;
using MimicMod.Survivors.Mimic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MimicMod.SkillStates.Secondary
{
    public class ThrowTarBomb : GenericProjectileBaseState
    {

        public static float BaseDuration = 0.65f;
        public static float BaseDelayDuration = 0.35f * BaseDuration;
        private int numBombs = MimicStaticValues.numberTarBombs;

        public static GameObject oilPrefab = ((GenericProjectileBaseState)new ThrowBarrel()).projectilePrefab;

        public override void OnEnter()
        {
            numBombs += base.characterBody.GetBuffCount(MimicBuffs.bombBuff);

            base.projectilePrefab = oilPrefab;

            // TODO: Switch sound
            base.attackSoundString = FireBombardment.shootSoundString;
            
            base.baseDuration = BaseDuration;
            base.baseDelayBeforeFiringProjectile = BaseDelayDuration;

            base.damageCoefficient = MimicStaticValues.tarBombDamageCoeff;
            base.force = Random.Range(40f, 80f);
            base.recoilAmplitude = 0.1f;
            base.bloom = 10;
            
            base.OnEnter();
            Fire();
        }

        private void Fire()
        {
            if (!this.isAuthority)
            {
                return;
            }

            Ray aimRay = ((BaseState)this).GetAimRay();
            FireProjectileInfo val = default(FireProjectileInfo);

            if (projectilePrefab != null)
            {
                for (int i = 0; i < numBombs; i++)
                {
                    val.projectilePrefab = projectilePrefab;
                    val.position = aimRay.origin;
                    val.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimRay.direction, 0f, 15f, 1f, 1f, 0f, 0f));
                    val.owner = this.gameObject;
                    val.damage = this.damageStat * base.damageCoefficient;
                    val.damageTypeOverride = DamageType.ClayGoo;
                    val.force = force * Random.Range(0.1f, 0.75f);
                    val.crit = this.RollCrit();
                    val.speedOverride = Random.Range(40f, 80f);
                    ProjectileManager.instance.FireProjectile(val);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void PlayAnimation(float duration)
        {

            if (base.GetModelAnimator())
            {
                base.PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", this.duration);
            }
        }
    }
}