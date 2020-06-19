namespace FanControl
{
    using OpenHardwareMonitor.Hardware;
    using System;
    using System.Drawing;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            var fan_icon = new TrayIcon("./fan.png");
            var fan2_icon = new TrayIcon("./fan.png");
            var cpu_icon = new TrayIcon("./cpu.png");
            var gpu_icon = new TrayIcon("./gpu.png");

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

            var fan_rpm = 0;
            var fan2_rpm = 0;

            while (true)
            {
                Console.Clear();

                Console.WriteLine("Max: " + monitor.MaxTemp() + " Avg: " + monitor.AverageTemp());
                Console.WriteLine(monitor.GetCPUTemps());

                foreach (var temp in monitor.CpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + (int)temp.Value);
                }

                foreach (var temp in monitor.GpuTemps)
                {
                    Console.WriteLine(temp.Key + ": " + (int)temp.Value);
                }

                byte speed = 0;
                byte speed2 = 0;

                //double currentTemp = monitor.MaxTemp();
                double currentTemp = monitor.AverageTemp();

                if (currentTemp > 65.0) speed = 1;
                if (currentTemp > 70.0) speed = 2;
                if (currentTemp > 74.0) speed = 3;
                if (currentTemp > 78.0) speed = 4;

                if (speed > 0) speed2 = (byte)(speed - 1);

                if (currentTemp > 80.0 || monitor.MaxTemp() > 85)
                {
                    speed = 7;
                    speed2 = 7;
                }

                var fan_percentage = (int)(speed / 7.0 * 100.0);
                fan_icon.Update(fan_percentage.ToString(), ColorFromDouble(fan_percentage), "Fan1: " + fan_rpm.ToString() + " rpm");

                var fan2_percentage = (int)(speed2 / 7.0 * 100.0);
                fan2_icon.Update(fan2_percentage.ToString(), ColorFromDouble(fan2_percentage), "Fan2: " + fan2_rpm.ToString() + " rpm");

                cpu_icon.Update(monitor.CPUAverageTemp().ToString(), ColorFromDouble(monitor.CPUAverageTemp()));
                gpu_icon.Update(monitor.GPUAverageTemp().ToString(), ColorFromDouble(monitor.GPUAverageTemp()));

                comm.AcquireLock(100);

                Thread.Sleep(10);
                fan.SetTargetSpeed(speed);
                Thread.Sleep(10);
                fan2.SetTargetSpeed(speed2);

                Thread.Sleep(100);

                var fanSpeed = fan.GetCurrentSpeed();
                var fan2Speed = fan2.GetCurrentSpeed();

                fan_rpm = fan.GetRPM();
                fan2_rpm = fan2.GetRPM();

                Console.WriteLine("fan: " + fanSpeed + " " + fan_rpm);
                Console.WriteLine("fan2: " + fan2Speed + " " + fan2_rpm);

                comm.ReleaseLock();

                Thread.Sleep(3000);
            }
        }

        private static Color ColorFromDouble(double temp)
        {
            if (temp < 10) return Color.White;
            if (temp < 65) return Color.Green;
            if (temp < 75) return Color.Orange;
            return Color.Red;
        }
    }
}
