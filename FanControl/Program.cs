namespace FanControl
{
    using OpenHardwareMonitor.Hardware;
    using System;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var computer = new Computer
            {
                CPUEnabled = true,
                GPUEnabled = true,
                //FanControllerEnabled = true
            };
            computer.Open();

            var monitor = new HardwareMonitorWrapper(computer);
            var comm = new EmbeddedControllerCommunicator(computer);

            var fan = new Fan(comm);
            var fan2 = new Fan(comm, 1);

            while (true)
            {
                Console.Clear();
                double currentMaxTemp = 60;

                foreach (var temp in monitor.CpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + temp.Value);
                    if (temp.Value > currentMaxTemp) currentMaxTemp = temp.Value;
                }

                foreach (var temp in monitor.GpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + temp.Value);
                    if (temp.Value > currentMaxTemp) currentMaxTemp = temp.Value;
                }

                comm.AcquireLock(100);

                byte speed = 0;

                if (currentMaxTemp > 65.0) speed = 1;
                if (currentMaxTemp > 70.0) speed = 3;
                if (currentMaxTemp > 78.0) speed = 7;

                fan.SetTargetSpeed(speed);
                fan2.SetTargetSpeed(speed);

                Thread.Sleep(100);

                var fanSpeed = fan.GetCurrentSpeed();
                var fan2Speed = fan2.GetCurrentSpeed();

                Console.WriteLine("fan: " + fanSpeed + " " + fan.GetRPM());
                Console.WriteLine("fan2: " + fan2Speed + " " + fan2.GetRPM());

                comm.ReleaseLock();

                Thread.Sleep(3000);
            }



            Console.ReadLine();
        }
    }
}
