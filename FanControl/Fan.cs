using OpenHardwareMonitor.Hardware;

namespace FanControl
{
    public class Fan
    {
        private const byte READ_REG = 0x2f;
        private const byte WRITE_REG = 0x2f;

        // min = 0, max = 7
        private const byte FAN_SPEED_RESET = 128;

        private readonly EmbeddedControllerCommunicator comm;
        
        public float CurrentSpeed { get; private set; }

        private byte offset;

        public Fan(EmbeddedControllerCommunicator comm, byte offset = 0)
        {
            this.comm = comm;
            this.offset = offset;
        }

        public void SetTargetSpeed(byte speed)
        {
            this.comm.WriteByte(0x31, this.offset);
            this.comm.WriteByte((byte)(WRITE_REG + this.offset), speed);
        }

        public int GetCurrentSpeed()
        {
            this.comm.WriteByte(0x31, this.offset);
            int val = this.comm.ReadByte((byte)(READ_REG + this.offset));
            return val;
        }

        public int GetRPM()
        {
            this.comm.WriteByte(0x31, this.offset);
            int val = this.comm.ReadByte(0x84);
            val |= this.comm.ReadByte(0x84 + 1) << 8;
            return val;
        }

        public void Reset()
        {
            this.comm.WriteByte((byte)(WRITE_REG + this.offset), FAN_SPEED_RESET);
        }
    }
}
