using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;


namespace SizeMattersFishing.Structs;

[StructLayout(LayoutKind.Explicit, Size = 552)]
public unsafe struct AddonSpearfishing
{
    [FieldOffset(0x0)] public AtkUnitBase AtkUnitBase;
}