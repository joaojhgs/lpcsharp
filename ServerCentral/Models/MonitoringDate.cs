namespace ServerCentral.Models
{
    public class MonitoringData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double CpuUsage { get; set; }
        public double MemoryUsed { get; set; }
        public double MemoryFree { get; set; }
        public double DiskRead { get; set; }
        public double DiskWrite { get; set; }
    }
}
