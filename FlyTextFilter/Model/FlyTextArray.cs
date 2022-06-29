using System.Runtime.InteropServices;

namespace FlyTextFilter.Model;

[StructLayout(LayoutKind.Explicit, Size = 0x30 * 10)]
public struct FlyTextArray
{
    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public struct FlyTextGroup
    {
        // linked list of nodes in addon + 0x238 (+ 0x230 is the first res node)
        // 54 * 0x48 followed by 76 * 0x48 (static vs dynamic)
        // first 0x40 bytes of each are for all the nodes of the component node
        [FieldOffset(0x00)] public unsafe long* LinkedList; // **(long**)(*LinkedList + 0x10) == AtkComponentNode
        [FieldOffset(0x08)] public long CurrentNbOfNodes;
        [FieldOffset(0x10)] public float X;
        [FieldOffset(0x14)] public float Y;
        [FieldOffset(0x18)] public float HorizontalTranslation;
        [FieldOffset(0x1C)] public float VerticalTranslation;
        [FieldOffset(0x20)] public float VerticalOffset;
        [FieldOffset(0x24)] public short MaxNbOfNodes; // will delete the oldest node if this number is reached
        [FieldOffset(0x26)] public short Priority1; // (== 9 - group index) (9->0)
        [FieldOffset(0x28)] public short Priority2; // final priority == ((5000 * Priority1) + Priority2)
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
