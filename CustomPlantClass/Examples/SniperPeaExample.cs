namespace CustomPlantClass.Examples
{
    public class SniperPea_Example : BaseCustomPlant
    {
        public SniperPea plant => gameObject.GetComponent<SniperPea>();
        public override Bullet Shoot_Custom()
        {
            if (plant == null || plant.board == null)
                return null;

            Zombie target = plant.targetZombie;

            if (target == null || !plant.SearchUniqueZombie(target))
            {
                var z = plant.SearchZombie();
                if (z == null) return null;
                target = z.GetComponent<Zombie>();
            }

            if (target == null)
                return null;

            plant.attackCount++;
            GameAPP.PlaySound(0x28, 0.2f, 1f);

            int damage = plant.attackDamage;
            if (plant.attackCount % 6 == 0)
            {
                damage = 1000000;
            }

            AttackEffect(target, damage);

            if (target.theStatus == ZombieStatus.Dying || target.beforeDying)
                plant.targetZombie = null;
            return null;
        }
        public virtual Zombie AttackEffect(Zombie zombie, int dmg)
        {
            zombie.TakeDamage(dmg, plant.Cast<IDamageMaker>(), DamageType.NormalAll, plant.thePlantType);
            return zombie;
        }
        public override string GetTextString() => $"充能 : {plant.attackCount % 6} / 6";
    }
}