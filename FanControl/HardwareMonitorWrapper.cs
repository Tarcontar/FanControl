namespace FanControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OpenHardwareMonitor.Hardware;

    public class HardwareMonitorWrapper
    {
        private Computer computer;
        private List<IHardware> cpus;
        private List<List<ISensor>> cpuTempSensors;
        private List<IHardware> gpus;
        private List<List<ISensor>> gpuTempSensors;

        public HardwareMonitorWrapper(Computer computer)
        {
            this.computer = computer;

            InitializeCpuSensors();
            InitializeGpuSensors();
        }

        public List<KeyValuePair<string, double>> CpuTemps
        {
            get
            {
                var results = new List<KeyValuePair<string, double>>();

                for (int i = 0; i < this.cpus.Count; i++)
                {
                    this.cpus[i].Update();
                    results.Add(new KeyValuePair<string, double>(
                        this.cpus[i].Name,
                        GetAverageTemperature(this.cpuTempSensors[i])));
                }

                return results;
            }
        }

        public string GetCPUTemps()
        {
            var temps = "";

            foreach (var cpu_sensors in cpuTempSensors)
            {
                foreach (var sensor in cpu_sensors)
                {
                    if (sensor.Value != null) temps += sensor.Name + ": " + (float)sensor.Value + "\n";
                }
            }

            return temps;
        }

        public double MaxTemp()
        {
            var maxTemp = 0.0f;

            foreach (var cpu_sensors in cpuTempSensors)
            {
                foreach (var sensor in cpu_sensors)
                {
                    if (sensor.Value != null && sensor.Value > maxTemp) maxTemp = (float)sensor.Value;
                }
            }

            foreach (var gpu_sensors in gpuTempSensors)
            {
                foreach (var sensor in gpu_sensors)
                {
                    if (sensor.Value != null && sensor.Value > maxTemp) maxTemp = (float)sensor.Value;
                }
            }

            return maxTemp;
        }

        public double CPUAverageTemp()
        {
            var sum = 0.0f;
            var count = 0;

            foreach (var cpu_sensors in cpuTempSensors)
            {
                foreach (var sensor in cpu_sensors)
                {
                    if (sensor.Value != null)
                    {
                        sum += (float)sensor.Value;
                        count++;
                    }
                }
            }

            return sum / count;
        }

        public double GPUAverageTemp()
        {
            var sum = 0.0f;
            var count = 0;

            foreach (var gpu_sensors in gpuTempSensors)
            {
                foreach (var sensor in gpu_sensors)
                {
                    if (sensor.Value != null)
                    {
                        sum += (float)sensor.Value;
                        count++;
                    }
                }
            }

            return sum / count;
        }

        public double AverageTemp()
        {
            var sum = 0.0f;
            var count = 0;

            foreach (var cpu_sensors in cpuTempSensors)
            {
                foreach (var sensor in cpu_sensors)
                {
                    if (sensor.Value != null)
                    {
                        sum += (float)sensor.Value;
                        count++;
                    }
                }
            }

            foreach (var gpu_sensors in gpuTempSensors)
            {
                foreach (var sensor in gpu_sensors)
                {
                    if (sensor.Value != null)
                    {
                        sum += (float)sensor.Value;
                        count++;
                    }
                }
            }

            return sum / count;
        }

        public List<KeyValuePair<string, double>> GpuTemps
        {
            get
            {
                var results = new List<KeyValuePair<string, double>>();

                for (int i = 0; i < this.gpus.Count; i++)
                {
                    this.gpus[i].Update();
                    results.Add(new KeyValuePair<string, double>(
                        this.gpus[i].Name,
                        GetAverageTemperature(this.gpuTempSensors[i])));
                }

                return results;
            }
        }

        private static double GetAverageTemperature(List<ISensor> sensors)
        {
            double temperatureSum = 0;
            int count = 0;

            foreach (var sensor in sensors)
            {
                if (sensor.Value.HasValue)
                {
                    temperatureSum += sensor.Value.Value;
                    count++;
                }
            }

            return temperatureSum / count;
        }

        private void InitializeCpuSensors()
        {
            this.cpus = GetHardware(HardwareType.CPU);
            this.cpuTempSensors = new List<List<ISensor>>();
            int sensorsTotal = 0;

            for (int i = 0; i < this.cpus.Count; i++)
            {
                var sensors = GetCpuTemperatureSensors(this.cpus[i]);
                sensorsTotal += sensors.Count;
                this.cpuTempSensors.Add(sensors);
            }

            if (sensorsTotal <= 0)
            {
                throw new PlatformNotSupportedException("Failed to access CPU temperature sensors(s).");
            }
        }

        private void InitializeGpuSensors()
        {
            var list = new List<IHardware>();
            list.AddRange(GetHardware(HardwareType.GpuAti));
            list.AddRange(GetHardware(HardwareType.GpuNvidia));

            this.gpus = list;
            this.gpuTempSensors = new List<List<ISensor>>();
            int sensorsTotal = 0;

            for (int i = 0; i < this.gpus.Count; i++)
            {
                var sensors = GetGpuTemperatureSensors(this.gpus[i]);
                sensorsTotal += sensors.Count;
                this.gpuTempSensors.Add(sensors);
            }

            if (sensorsTotal <= 0)
            {
                throw new PlatformNotSupportedException("Failed to access GPU temperature sensors(s).");
            }
        }

        private static List<ISensor> GetGpuTemperatureSensors(IHardware gpu)
        {
            return gpu.Sensors.Where(x => x.SensorType == SensorType.Temperature).ToList();
        }

        private static List<ISensor> GetCpuTemperatureSensors(IHardware cpu)
        {
            var sensors = new List<ISensor>();
            cpu.Update();

            foreach (ISensor s in cpu.Sensors)
            {
                if (s.SensorType == SensorType.Temperature)
                {
                    string name = s.Name.ToUpper();

                    //if (name.Contains("PACKAGE") || name.Contains("TOTAL"))
                    //{
                    //    return new List<ISensor> { s };
                    //}
                    //else
                    //{
                        sensors.Add(s);
                    //}
                }
            }

            return sensors.ToList();
        }

        private List<IHardware> GetHardware(HardwareType type)
        {
            return this.computer.Hardware.Where(x => x.HardwareType == type).ToList();
        }
    }
}
