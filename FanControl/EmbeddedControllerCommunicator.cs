using OpenHardwareMonitor.Hardware;
using System.Threading;

namespace FanControl
{
    public class EmbeddedControllerCommunicator
    {
        const byte CommandPort = 0x66;
        const byte DataPort = 0x62;

        const int RWTimeout = 500;
        const int FailuresBeforeSkip = 20;
        const int MaxRetries = 5;

        // See ACPI specs ch.12.2
        enum ECStatus : byte
        {
            OutputBufferFull = 0x01,    // EC_OBF
            InputBufferFull = 0x02,     // EC_IBF
            // 0x04 is ignored
            Command = 0x08,             // CMD
            BurstMode = 0x10,           // BURST
            SCIEventPending = 0x20,     // SCI_EVT
            SMIEventPending = 0x40      // SMI_EVT
            // 0x80 is ignored
        }

        // See ACPI specs ch.12.3
        enum ECCommand : byte
        {
            Read = 0x80,            // RD_EC
            Write = 0x81,           // WR_EC
            BurstEnable = 0x82,     // BE_EC
            BurstDisable = 0x83,    // BD_EC
            Query = 0x84            // QR_EC
        }

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

            WritePort(CommandPort, (byte)ECCommand.Read);

            if (!WaitWrite()) return false;

            WritePort(DataPort, register);

            if (!WaitWrite() || !WaitRead()) return false;

            value = ReadPort(DataPort);
            return true;
        }

        private bool TryWriteByte(byte register, byte value)
        {
            if (!WaitWrite()) return false;

            WritePort(CommandPort, (byte)ECCommand.Write);

            if (!WaitWrite()) return false;

            WritePort(DataPort, register);

            if (!WaitWrite()) return false;

            WritePort(DataPort, value);
            return true;
        }

        private bool WaitRead()
        {
            if (waitReadFailures > FailuresBeforeSkip)
            {
                return true;
            }
            else if (WaitForEcStatus(ECStatus.OutputBufferFull, true))
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

        private bool WaitWrite() => WaitForEcStatus(ECStatus.InputBufferFull, false);

        private bool WaitForEcStatus(ECStatus status, bool isSet)
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                timeout--;
                byte value = ReadPort(CommandPort);

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
