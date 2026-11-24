using System.Runtime.InteropServices;

namespace IGOR.Interface.Logging
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PlcError
    {
        [FieldOffset(0)]
        public uint Source;
        [FieldOffset(4)]
        public uint Type;
        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string Msg;
    }
}
