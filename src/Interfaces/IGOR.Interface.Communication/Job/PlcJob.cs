using System.Runtime.InteropServices;

namespace IGOR.Interface.Communication.Job
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PlcJob
    {
        private static uint jobId;
        [FieldOffset(0)]
        public uint JobID;
        [FieldOffset(4)]
        public MoveType MoveType;

        [FieldOffset(8)]
        public double StartPosX;
        [FieldOffset(16)]
        public double StartPosY;
        [FieldOffset(24)]
        public double EndPosX;
        [FieldOffset(32)]
        public double EndPosY;
        [FieldOffset(40)]
        public bool IsBlack;

        public PlcJob(MoveType moveType, double startX, double startY, double endX, double endY, bool isBlack)
        {
            JobID = ++jobId;
            MoveType = moveType;
            StartPosX = startX;
            StartPosY = startY;
            EndPosX = endX;
            EndPosY = endY;
            IsBlack = isBlack; 
        }
    }
}
