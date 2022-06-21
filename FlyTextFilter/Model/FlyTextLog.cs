using System;
using Dalamud.Game.Gui.FlyText;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace FlyTextFilter.Model;

public class FlyTextLog
{
    public FlyTextCreationSource FlyTextCreationSource;
    public FlyTextKind FlyTextKind;
    public FlyTextCharCategory SourceCategory;
    public FlyTextCharCategory TargetCategory;
    public int Option;
    public int ActionKind;
    public int ActionId;
    public int Val;
    public int Val1;
    public int Val2;
    public int Val3;
    public int Val4;
    public uint ItemId;
    public int Count;
    public bool HasSourceBeenAdjusted;

    public override bool Equals(object? logObj)
    {
        return logObj is FlyTextLog flyTextLog
               && this.FlyTextCreationSource == flyTextLog.FlyTextCreationSource
               && this.FlyTextKind == flyTextLog.FlyTextKind
               && this.Option == flyTextLog.Option
               && this.ActionKind == flyTextLog.ActionKind
               && this.ActionId == flyTextLog.ActionId
               && this.Val == flyTextLog.Val
               && this.Val1 == flyTextLog.Val1
               && this.Val2 == flyTextLog.Val2
               && this.Val3 == flyTextLog.Val3
               && this.Val4 == flyTextLog.Val4
               && this.ItemId == flyTextLog.ItemId
               && this.Count == flyTextLog.Count
               && this.HasSourceBeenAdjusted == flyTextLog.HasSourceBeenAdjusted;
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add((int)this.FlyTextCreationSource);
        hashCode.Add((int)this.FlyTextKind);
        hashCode.Add(this.Option);
        hashCode.Add(this.ActionKind);
        hashCode.Add(this.ActionId);
        hashCode.Add(this.Val);
        hashCode.Add(this.Val1);
        hashCode.Add(this.Val2);
        hashCode.Add(this.Val3);
        hashCode.Add(this.Val4);
        hashCode.Add(this.ItemId);
        hashCode.Add(this.Count);
        hashCode.Add(this.HasSourceBeenAdjusted);
        return hashCode.ToHashCode();
    }
}
