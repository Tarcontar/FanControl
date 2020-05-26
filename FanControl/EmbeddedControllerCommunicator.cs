using OpenHardwareMonitor.Hardware;
using System.Threading;

namespace FanControl
{
    public class EmbeddedControllerCommunicator
    {
        const byte COMMAND_PORT = 0x66;
        const byte DATA_PORT = 0x62;

        const byte OUTPUT_BUFFER_FULL = 0x01;
        const byte INPUT_BUFFER_FULL = 0x02;

        const byte READ_COMMAND = 0x80;
        const byte WRITE_COMMAND = 0x81;

        const int RWTimeout = 500;
        const int FailuresBeforeSkip = 20;
        const int MaxRetries = 5;


        int waitReadFailures = 0;
        private Computer computer;

        public EmbeddedControllerCommunicator(Computer computer)
        {
            this.computer = computer;
        }

        public void WriteByte(byte register, byte value)
        {
            int writes = 0;

            while (writes < MaxRetries && !TryWriteByte(register, value))
            {
                writes++;
            }
        }

        public byte ReadByte(byte register)
        {
            byte result = 0;
            int reads = 0;

            while (reads < MaxRetries && !TryReadByte(register, out result))
            {
                reads++;
            }

            return result;
        }

        public bool AcquireLock(int timeout)
        {
            return this.computer.WaitIsaBusMutex(timeout);
        }

        public void ReleaseLock()
        {
            this.computer.ReleaseIsaBusMutex();
        }

        private bool TryReadByte(byte register, out byte value)
        {
            value = 0x0;

            if (!WaitWrite()) return false;

            WritePort(COMMAND_PORT, READ_COMMAND);

            if (!WaitWrite()) return false;

            WritePort(DATA_PORT, register);

            if (!WaitWrite() || !WaitRead()) return false;

            value = ReadPort(DATA_PORT);
            return true;
        }

        private bool TryWriteByte(byte register, byte value)
        {
            if (!WaitWrite()) return false;

            WritePort(COMMAND_PORT, WRITE_COMMAND);

            if (!WaitWrite()) return false;

            WritePort(DATA_PORT, register);

            if (!WaitWrite()) return false;

            WritePort(DATA_PORT, value);
            return true;
        }

        private bool WaitRead()
        {
            if (waitReadFailures > FailuresBeforeSkip)
            {
                return true;
            }
            else if (WaitForEcStatus(OUTPUT_BUFFER_FULL, true))
            {
                waitReadFailures = 0;
                return true;
            }
            else
            {
                waitReadFailures++;
                return false;
            }
        }

        private bool WaitWrite() => WaitForEcStatus(INPUT_BUFFER_FULL, false);

        private bool WaitForEcStatus(byte status, bool isSet)
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                timeout--;
                byte value = ReadPort(COMMAND_PORT);

                if (isSet)
                {
                    value = (byte)~value;
                }

                if (((byte)status & value) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void WritePort(int port, byte value) => this.computer.WriteIoPort(port, value);
        private byte ReadPort(int port) => this.computer.ReadIoPort(port);
    }
}
