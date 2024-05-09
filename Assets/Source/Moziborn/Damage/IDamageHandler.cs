namespace Moziborn.Damage
{
    public interface IDamageHandler
    {
        DamageFactionId factionId { get; }
        
        float TakeDamage(float damageAmount);
    }
}