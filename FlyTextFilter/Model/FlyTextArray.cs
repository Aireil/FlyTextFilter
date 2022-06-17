using System.Runtime.InteropServices;

namespace FlyTextFilter.Model;

[StructLayout(LayoutKind.Explicit, Size = 0x30 * 10)]
public struct FlyTextArray
{
    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public struct FlyTextGroup
    {
        [FieldOffset(0x10)] public float X;
        [FieldOffset(0x14)] public float Y;
        [FieldOffset(0x18)] public float HorizontalTranslation;
        [FieldOffset(0x1C)] public float VerticalTranslation;
        [FieldOffset(0x20)] public float SomethingAboutUiScale;
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
