using Cysharp.Threading.Tasks;

namespace CustomPlantClass.Examples
{
    public class ArmedChomperBase : CustomShooter
    {
	    public GameObject changeSprite { get; }
	    public Transform shoot3;
        protected Sprite WallnutSprite;
        protected override bool OverrideDamagePipeline => true;
        protected override int DamageLimit => _plant.thePlantMaxHealth/3;
        public int ShootTimes = 0;
        public bool IsSuper = false;
        public override int OnTakeDamage(int damage, IDamageMaker damageFrom, DamageType damageType)
        {
            ReplaceSprite();
            return base.OnTakeDamage(damage, damageFrom, damageType);
        }
        public virtual void ReplaceSprite()
        {
            Lawnf.ChangeSprite(_plant.thePlantHealth,_plant.thePlantMaxHealth,changeSprite);
        }
        public virtual Color LaserStartColor => new(0.6f,0,1,1);
        public virtual Color LaserEndColor => LaserStartColor;
        public override Bullet Shoot_Custom()
        {
            var condition = CheckZombie;
            Zombie target=Lawnf.GetNearestZombie(_plant.board,_plant.shoot.position,condition);
            var col = target?.col;
            if (col != null)
            {
                _plant.targetZombie=target;
                var pos = col.bounds.center;
                var shoot = _plant.shoot.position;
                int row = Mathf.Max(target.theZombieRow,_plant.thePlantRow);
                Core.Lawnf.FromTo
                (
                    shoot,pos,row,_plant.board.transform,
                    _plant.board.GetCancellationTokenOnDestroy(),
                    LaserStartColor,LaserEndColor
                );
                ShootTimes++;
                if(ShootTimes < 6 - _plant.shootingLevel)
                {
                    target.TakeDamage
                    (
                        _plant.attackDamage,
                        _plant.Cast<IDamageMaker>(),
                        DamageType.Shieldless,
                        _plant.thePlantType
                    );
                    SuperEnd();
                }
                else
                {
                    target.TakeDamage
                    (
                        1000000,
                        _plant.Cast<IDamageMaker>(),
                        DamageType.MaxDamage,
                        _plant.thePlantType
                    );
                    StartPF();
                    ShootTimes=0;
                    _plant.Recover(_plant.thePlantMaxHealth);
                }
            }
            GameAPP.PlaySound(SoundType.CherryBomb,0.2f);
            _plant.anim.SetTrigger("shoot2");
            return null;
        }
        public virtual bool CheckZombie(Zombie z)
        {
            return z != null && z.col != null && z.axis.position.x > _plant.shoot.position.x && (Lawnf.InLandStatus(z.theStatus) || z.theStatus==ZombieStatus.Flying) && z.Alive && z.gameObject != null && z.Team != _plant.Team;
        }
        public override BulletType GetBulletType() => BulletType.Bullet_pea_chomper;
        public override BulletType GetBulletType2() => GetBulletType();
        public IEnumerator SuperShoot_Custom()
        {
            List<Transform> shoots = new(){_plant.shoot2,shoot3};
            for ( int i = 0 ; i < 8 ; i++)
            {
                for ( int j = 0 ; j < 3 ; j++)
                {
                    var loc = shoots.GetRandomItem().position;
                    int dmg=_plant.attackDamage;
                    PlantMgr.SetBullet(_plant,loc,GetBulletType2(),BulletMoveWay.SuperGatling,(int)((dmg + ((dmg>>31) & 3U)) >> 2),Vector2.zero,Random.Range(-15f, 15f)).normalSpeed=Random.Range(12f,14f);
                }
                yield return new WaitForSeconds(0.05f);
            }
            var nut=CreatePlant.Instance.SetPlant(_plant.thePlantColumn,_plant.thePlantRow,PlantType.HugeWallNut);
            if (nut != null)
            {
                nut.damageAdder=_plant.damageAdder;
                nut.ModifyDamage(PlantDamageAdder.Update,0);
                var renderer = nut.GetComponentInChildren<SpriteRenderer>();
                if(renderer!= null)
                {
                    renderer.sprite=WallnutSprite;
                }
                if(nut is BigWallNut)
                {
                    var a = (Zombie z) => z.KnockBack(1);
                    (nut as BigWallNut).onCrash=a;
                }
            }
        }
        public override void StartPF()
        {
            _plant.invincible = true;
            _plant.uncrashable = true;
            isPF = true;
            _plant.isFlashing = true;
        }
        public override void SuperEnd()
        {
            _plant.invincible = false;
            _plant.uncrashable = false;
            isPF = false;
            _plant.flashCountDown = 0f;
            _plant.isFlashing = false;
        }
        public override Bullet Shoot2_Custom()
        {
            if (!isPF)
            {
                int dmg=_plant.attackDamage;
                var bullet=PlantMgr.SetBullet(
                    _plant,_plant.shoot2.position,
                    GetBulletType(),BulletMoveWay.Track,
                    (int)((dmg + ((dmg>>31) & 3U)) >> 2)
                );
                bullet.targetZombie=_plant.targetZombie;
                var bullet2=PlantMgr.SetBullet(
                    _plant,shoot3.position,
                    GetBulletType(),BulletMoveWay.Track,
                    (int)((dmg + ((dmg>>31) & 3U)) >> 2)
                );
                bullet2.targetZombie=_plant.targetZombie;
                return bullet;
            }
            else
            {
                _plant.StartCoroutine(SuperShoot_Custom());
                return null;
            }
        }
        public override GameObject SearchZombie()
        {
            Board board = _plant.board;
            if (board == null || board.zombieArray == null)
                return null;

            foreach (Zombie z in board.zombieArray)
            {
                if (z == null)
                    continue;

                // Skip same-team zombies
                if (z.Team == _plant.Team)
                    continue;

                // Must have axis transform
                if (z.axis == null)
                    continue;

                Vector3 zombiePos = z.axis.position;

                // Must be within vision range
                float vision = _plant.vision;
                if (zombiePos.x >= vision || zombiePos.x == vision)
                    continue;

                // Must be in front of plant (axis.x <= zombie.x and not equal)
                if (_plant.axis == null)
                    return null;

                Vector3 plantPos = _plant.axis.position;
                if (!(plantPos.x <= zombiePos.x && zombiePos.x != plantPos.x))
                    continue;

                // Must pass SearchUniqueZombie() check
                if (!_plant.SearchUniqueZombie(z))
                    continue;

                // Return zombie GameObject
                return z.gameObject;
            }

            return null;
        }
    }
    public class ArmedComperBulletBase : BaseCustomBullet
    {
        public override bool HitZombie(Zombie zombie)
        {
            SetParticle(_bullet.col.bounds.center,11);
            GameAPP.PlaySound(SoundType.BigChomp);
            if (!TypeMgr.IsBossZombie(zombie.theZombieType) && PlantMgr.GetPercent(12.5f))
            {
                zombie.Die(2);
            }
            else
            {
                zombie.TakeDamage(_bullet.Damage,_bullet.Cast<IDamageMaker>(),DamageType.NormalAll,_bullet.fromType);
            }
            _bullet.Die();
            return false;
        }
        public override ParticleType GetParticleType() => ParticleType.BigChomp;
    }
}