namespace FlyTextFilter.Model.FlyTextAdjustments;

public class FlyTextPositions
{
    public float? HealingGroupX;
    public float? HealingGroupY;
    public float? StatusDamageGroupX;
    public float? StatusDamageGroupY;

    public static FlyTextPositions GetDefaultPositions()
    {
        var (width, height) = Util.GetScreenSize();
        return new FlyTextPositions
        {
            HealingGroupX = width * (49.0f / 100.0f),
            HealingGroupY = height / 2.0f,
            StatusDamageGroupX = width * (11.0f / 20.0f),
            StatusDamageGroupY = height / 2.0f,
        };
    }
}
