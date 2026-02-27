using UnityEngine;

public interface ITileEffectHandler
{
    void ExecuteEnterEffect(Unit targetUnit);
    void ExecuteUpdateEffect(Unit targetUnit);
    void ExecuteExitEffect(Unit targetUnit);
    void ExecuteDeathEffect(Unit targetUnit);
    void ExecuteWaveClearEffect(Unit targetUnit);
}
