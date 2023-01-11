namespace FlyTextFilter.Model.FlyTextAdjustments;

public class FlyTextAdjustments
{
    public bool ShouldHideDamageTypeIconAutoAttacks = false;
    public bool ShouldHideDamageTypeIconStatusEffects = false;
    public bool ShouldHideDamageTypeIconOthers = false;
    public FlyTextPositions FlyTextPositions = new();
    public float? FlyTextScale = null;
    public float? PopupTextScale = null;
}
