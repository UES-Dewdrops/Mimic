using EntityStates;
using EntityStates.ClayBoss.ClayBossWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MimicMod.Survivors.Mimic;

namespace MimicMod.SkillStates.Secondary
{
    public class ThrowFireBomb : GenericProjectileBaseState
    {

        public static float BaseDuration = 0.65f;
        public static float BaseDelayDuration = 0.35f * BaseDuration;
        private float dmgBoost = 1f;
        private int numBombs = MimicStaticValues.numberFireBombs;

        public override void OnEnter()
        {

            numBombs += base.characterBody.GetBuffCount(MimicBuffs.bombBuff);


            base.projectilePrefab = FireBombardment.projectilePrefab;

            base.attackSoundString = FireBombardment.shootSoundString;
      
            base.baseDuration = BaseDuration;
            base.baseDelayBeforeFiringProjectile = BaseDelayDuration;

            base.damageCoefficient = MimicStaticValues.bombDamageCoefficient * dmgBoost;
            base.force = 80f;
            base.recoilAmplitude = 0.5f;
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
                    val.damageTypeOverride = DamageType.IgniteOnHit; // previously 512 
                    val.force = force * Random.Range(0.1f, 0.75f);
                    val.crit = this.RollCrit();
                    val.speedOverride = Random.Range(60f, 120f);
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