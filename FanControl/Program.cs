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
                foreach (var temp in monitor.CpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + temp.Value);
                }

                foreach (var temp in monitor.GpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + temp.Value);
                }

                comm.AcquireLock(100);

                fan.SetTargetSpeed(0);
                fan2.SetTargetSpeed(0);

                Thread.Sleep(100);

                Console.WriteLine("fan: " + fan.GetCurrentSpeed() + " " + fan.GetRPM());
                Console.WriteLine("fan2: " + fan2.GetCurrentSpeed() + " " + fan2.GetRPM());

                comm.ReleaseLock();

                Thread.Sleep(3000);
            }



            Console.ReadLine();
        }
    }
}
