using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class SystemMonitor
{
    private static System.Timers.Timer _timer = new System.Timers.Timer(5000); // Intervalo de 5 segundos
    private static long _prevIdleTime = 0;
    private static long _prevTotalTime = 0;

    public static async Task Main(string[] args)
    {
        _timer.Elapsed += async (sender, e) => await CollectAndSendData();
        _timer.Start();

        Console.WriteLine("Agente de monitoramento iniciado. Pressione Enter para sair.");
        Console.ReadLine();
        _timer.Stop();
    }

    private static async Task CollectAndSendData()
    {
        var Memory = GetMemoryUsage();
        var Disk = GetDiskUsage();
        var data = new
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsed = Memory.Used,
            MemoryFree = Memory.Free,
            DiskRead = Disk.Read,
            DiskWrite = Disk.Write,
        };

        await SendDataToServer(data);
    }

    private static double GetCpuUsage()
    {
        try
        {
            string[] cpuStats = File.ReadAllLines("/proc/stat")[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            
            // Posição das métricas no /proc/stat
            long user = long.Parse(cpuStats[1]);
            long nice = long.Parse(cpuStats[2]);
            long system = long.Parse(cpuStats[3]);
            long idle = long.Parse(cpuStats[4]);
            long iowait = long.Parse(cpuStats[5]);
            long irq = long.Parse(cpuStats[6]);
            long softirq = long.Parse(cpuStats[7]);

            long totalIdleTime = idle + iowait;
            long totalCpuTime = user + nice + system + totalIdleTime + irq + softirq;

            long idleDiff = totalIdleTime - _prevIdleTime;
            long totalDiff = totalCpuTime - _prevTotalTime;

            _prevIdleTime = totalIdleTime;
            _prevTotalTime = totalCpuTime;

            return totalDiff == 0 ? 0 : Math.Round((1.0 - (double)idleDiff / totalDiff) * 100.0, 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao calcular uso de CPU: {ex.Message}");
            return 0.0;
        }
    }

    private static (double Used, double Free) GetMemoryUsage()
    {
        string[] memInfo = File.ReadAllLines("/proc/meminfo");
        long totalMem = long.Parse(memInfo[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)[1]); // Total memória
        long freeMem = long.Parse(memInfo[1].Split(" ", StringSplitOptions.RemoveEmptyEntries)[1]);  // Memória livre

        double used = (totalMem - freeMem) / 1024.0; // Convertendo para MB
        double free = freeMem / 1024.0; // Convertendo para MB

        return (Math.Round(used, 2), Math.Round(free, 2));
    }

    private static (double Read, double Write) GetDiskUsage()
    {
        try
        {
            string[] diskStats = File.ReadAllLines("/proc/diskstats");
            double readBytes = 0;
            double writeBytes = 0;

            foreach (var line in diskStats)
            {
                string[] stats = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (stats.Length > 13)
                {
                    readBytes += long.Parse(stats[5]) * 512; // sectors read * sector size (512 bytes)
                    writeBytes += long.Parse(stats[9]) * 512; // sectors written * sector size (512 bytes)
                }
            }

            return (Math.Round(readBytes / 1024 / 1024, 2), Math.Round(writeBytes / 1024 / 1024, 2)); // Convert to MB
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao calcular uso de disco: {ex.Message}");
            return (0, 0);
        }
    }

    private static async Task SendDataToServer(object data)
    {
        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri("http://localhost:5160/api/monitoring");
            var content = new StringContent(JsonSerializer.Serialize(data));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("", content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Dados enviados com sucesso.");
            }
            else
            {
                Console.WriteLine("Erro ao enviar dados: " + response.StatusCode);
            }
        }
    }
}
