namespace CustomPlantClass.Examples
{
    public class DoomSniper_Example : CustomShooter
    {
        public DoomSniper plant => GetComponent<DoomSniper>();
        public override Transform FindShoot() => transform.FindChild("PeaShooter_Head/gun_lower/Shoot");
        public override string GetTextString() =>
        $"大招充能 : {plant.attributeCount}/300\n" +
        $"狂热能量 : {plant.craze}/100\n" +
        $"狂热时间 : {Mathf.CeilToInt(plant.crazeTimer)}/8";
        public virtual bool CheckZombie(Zombie zombie)
        {
            if (zombie == null || zombie.isMindControlled)
            {
                return false;
            }

            var col = zombie.col;
            if (col == null)
            {
                return false;
            }
            if (!col.enabled)
            {
                return false;
            }

            bool dir = zombie.axis.position.x <= plant.shoot.position.x;
            if (dir)
            {
                return false;
            }
            return Lawnf.InLandStatus(zombie.theStatus);
        }
        public override GameObject SearchZombie()
        {
            foreach (var zombie in Lawnf.GetAllZombies())
            {
                if (zombie != null)
                {
                    Vector3 zombiePos = zombie.transform.position;
                    if (plant.vision > zombiePos.x)
                    {
                        Vector3 shootPos = plant.shoot.position;
                        if (plant.TryGetComponent<DoomSniper_Example>(out var _))
                        {
                            if (zombie.theStatus != ZombieStatus.Flying)
                            {
                                return zombie.gameObject;
                            }
                        }
                        else if (zombiePos.x > shootPos.x)
                        {
                            if (plant.SearchUniqueZombie(zombie) && Lawnf.InLandStatus(zombie.theStatus))
                            {
                                return zombie.gameObject;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public override Bullet Shoot_Custom()
        {
            GameAPP.PlaySound(140, 0.5f, Random.Range(0.9f, 1.1f));

            Func<Zombie, bool> func = CheckZombie;
            Zombie nearestZombie = Lawnf.GetNearestZombie(
                plant.board,
                plant.shoot.position,
                func);

            var shootFire = ParticleManager.Instance.SetParticle(ParticleType.ShootFire, plant.shoot.position, 11);

            if (nearestZombie != null && nearestZombie.col != null)
            {
                Vector2 direction = (nearestZombie.col.bounds.center - plant.shoot.position).normalized;

                RaycastHit2D[] hits = Physics2D.RaycastAll(
                    plant.shoot.position,
                    direction,
                    float.MaxValue,
                    plant.zombieLayer);

                shootFire.transform.rotation = MathHelper.DirectionToRotation(direction);

                int damage = plant.attackDamage;
                if (Random.Range(0, 10) == 5)
                    damage *= 6;

                var hitCount = 0;
                foreach (var hit in hits)
                {
                    if (hit.collider.TryGetComponent(out Zombie zombie))
                    {
                        var hasEmber = zombie.TryGetEffect<EmberEffect>(EffectType.Ember, out var _);
                        int finalDamage = hasEmber || Lawnf.TravelAdvanced(AdvBuff.EnumValue12002) ? damage * 6 : damage;
                        AttackEffect(zombie, finalDamage);
                        hitCount++;
                        if (MathHelper.ApproximatelyZero(plant.crazeTimer))
                            plant.craze++;
                    }
                }

                plant.attributeCount += hitCount;

                if (plant.attributeCount >= 301)
                {
                    plant.attributeCount = 0;
                    var dmg = plant.attackDamage * 72;
                    Action<Zombie> act = DoomOnZombie;
                    plant.board.boardAction.SetDoom(nearestZombie.Column, nearestZombie.theZombieRow, false, false, default, dmg * plant.shootingLevel + dmg,
                        0, act, true, plant.thePlantType);
                }

                if (plant.craze >= 100)
                {
                    plant.crazeTimer = 8;
                    plant.craze = 0;
                }
            }
            return null;
        }
        public virtual Zombie AttackEffect(Zombie zombie, int dmg)
        {
            zombie.TakeDamage(DmgType.NormalAll, dmg, plant.thePlantType);
            return zombie;
        }
        public virtual void DoomOnZombie(Zombie z)
        {

        }
    }
}