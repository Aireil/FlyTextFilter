namespace FlyTextFilter.Model.FlyTextAdjustments;

public class FlyTextAdjustments
{
    public bool ShouldHideDamageTypeIconAutoAttacks = false;
    public bool ShouldHideDamageTypeIconStatusEffects = false;
    public bool ShouldHideDamageTypeIconOthers = false;
    public FlyTextPositions FlyTextPositions = new();
    public float? FlyTextScale = null;
    public float? PopupTextScale = null;

    public bool IsDefaultScaling()
    {
        return this.FlyTextScale == null && this.PopupTextScale == null;
    }

    public bool IsDefaultPositions()
    {
        return this.FlyTextPositions.HealingGroupX == null
               && this.FlyTextPositions.HealingGroupY == null
               && this.FlyTextPositions.StatusDamageGroupX == null
               && this.FlyTextPositions.StatusDamageGroupY == null;
    }
}
