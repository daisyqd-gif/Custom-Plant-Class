namespace CustomPlantClass
{
    public class BaseCustomZombie : MonoBehaviour
    {
        public virtual float theZombieShootInterval { get; set; } = 1.5f;
        public virtual float theZombieShootCountDown { get; set; } = 0f;
        public Zombie _zombie => GetComponent<Zombie>();
        public TextMeshPro extraText;
        public TextMeshPro extraTextShadow;
        public virtual void Awake()
        {
            theZombieShootCountDown = theZombieShootInterval;
            OnAwake();
        }
        public virtual void OnAwake() { }
        public virtual void Start()
        {
            _zombie.shoot = GetShoot();
            OnSpawn();
        }

        public virtual void OnSpawn() { }
        public virtual void OnDie() { }
        public virtual void Update()
        {
            OnUpdate();
        }
        public virtual void OnUpdate() { }
        public virtual void Start_Custom(ref Zombie z) { }

        public virtual Transform FindShoot()
            => _zombie.transform.GetChild(0).Find("Shoot");

        public Transform GetShoot()
        {
            var s = FindShoot();
            return s != null ? s : _zombie.transform;
        }

        // Buffs simplified
        public int[] buffs = new int[3];

        public void SetBuff(int index, int id) => buffs[index] = id;

        public int GetBuff(int index) => buffs[index];

        public bool BuffActive(int index)
            => Lawnf.TravelAdvanced((AdvBuff)buffs[index]);

        public bool GetBuff1Active() => BuffActive(0);
        public bool GetBuff2Active() => BuffActive(1);
        public virtual Bullet AnimShoot_Custom() => Shoot_Custom();

        public virtual Bullet Shoot_Custom() => /*MakeBullet();*/ null;
        public virtual Bullet Shoot2_Custom() => /*MakeBullet();*/ null;
        public virtual void Explode_Custom() { }

        public virtual bool AttackEffect(Plant p) => true;
        public virtual void FixedUpdate()
        {
            ZombieShootUpdate();
            OnFixedUpdate();
        }
        public virtual void ZombieShootUpdate()
        {
            theZombieShootCountDown -= Time.deltaTime;
            if (theZombieShootCountDown <= 0f)
            {
                _zombie.anim.SetTriggerString("Shoot");
                theZombieShootCountDown = theZombieShootInterval;
            }
        }
        public virtual void OnFixedUpdate() { }
    }
}