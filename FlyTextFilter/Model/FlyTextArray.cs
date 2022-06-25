using System.Runtime.InteropServices;

namespace FlyTextFilter.Model;

[StructLayout(LayoutKind.Explicit, Size = 0x30 * 10)]
public struct FlyTextArray
{
    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public struct FlyTextGroup
    {
        [FieldOffset(0x00)] public long AtkRelatedDataStruct; // *(*((*DataStruct) + 0x10 * sizeof(long))) == AtkComponentNode
        [FieldOffset(0x08)] public long CurrentNbOfNodes;
        [FieldOffset(0x10)] public float X;
        [FieldOffset(0x14)] public float Y;
        [FieldOffset(0x18)] public float HorizontalTranslation;
        [FieldOffset(0x1C)] public float VerticalTranslation;
        [FieldOffset(0x20)] public float VerticalOffset;
        [FieldOffset(0x24)] public short MaxNbOfNodes; // will delete the oldest node if this number is reached
        [FieldOffset(0x26)] public short Priority1; // (== 9 - group index) (9->0)
        [FieldOffset(0x28)] public short Priority2; // Final priority == ((5000 * Priority1) + Priority2)
    }

    // [0] on self, healing
    // [1] on self, status-damage
    // [2] -> [9] unknown/dynamic
    public unsafe FlyTextGroup* this[int i]
    {
        get
        {
            if (i is < 0 or > 10)
                return null;

            fixed (void* ptr = &this)
            {
                return (FlyTextGroup*)ptr + i;
            }
        }
    }
}
