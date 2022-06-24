// ReSharper disable NonReadonlyMemberInGetHashCode
namespace FlyTextFilter.Model;

public class FlyTextLog
{
    public FlyTextCreation FlyTextCreation;
    public FlyTextCharCategory SourceCategory = FlyTextCharCategory.None;
    public FlyTextCharCategory TargetCategory = FlyTextCharCategory.None;
    public bool WasFiltered;
    public bool HasSourceBeenAdjusted;
    public bool IsPartial;
}
