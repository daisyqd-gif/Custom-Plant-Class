namespace CustomPlantClass
{
    public interface IRedirectAnimShoot
    {
        public Bullet Shoot1();
    }
    public interface IRedirectAnimShoot2
    {
        public Bullet Shoot2();
    }
    public interface IOverrideDamagePipeline
    {
        public int GetDamage(int damage, IDamageMaker damageFrom, DamageType theDamageType);
    }
    public interface ICustomClick
    {
        public void OnClicked(Mouse mouse);
    }
    public interface ICustomPF
    {
        public bool IsImmune { get; }
        public void StartPF();
        public void SuperEnd();
    }
    public interface IPlantDieRedirector
    {
        public bool CanBeCrashed { get; }
        public bool CanDie { get; }
        public bool CanBeFrozen { get; }
    }
    public interface IPlantDieHandler
    {
        public void OnDie(DieReason reason);
    }
    public interface IPlantTextHandler
    {
        public void InitText();
    }
}