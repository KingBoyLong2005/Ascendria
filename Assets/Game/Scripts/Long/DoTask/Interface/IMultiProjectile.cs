public interface IMultiProjectile
{
    int ProjectileCount { get; }  // Chỉ get
    void AddProjectile(int amount);
    // int MaxProjectileCount { get; } 
}
