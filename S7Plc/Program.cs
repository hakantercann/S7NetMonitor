using Newtonsoft.Json;
using S7.Net;
using S7Plc.Controllers;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options => options.AddPolicy("AllowAnyOrigin",
    builder =>
    {
        builder.AllowAnyOrigin()
       .AllowAnyMethod()
        .AllowAnyHeader();
    }));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<MyBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAnyOrigin");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class DataBlocks
{
    public int ConfigID { get; set; }
    public byte[] bytes { get; set; }
}
class BytesConfig
{
    public int ID { get; set; }
    public int db { get; set; }
    public int size { get; set; }
    public int type { get; set; }
}
public class MyBackgroundService : BackgroundService
{
    public enum ConnectionStatus
    {
        NotConnected = 1,
        Connected = 2,

    }

    string jsonStringBytesConfig = @"[
            { 'ID':6, 'db':8, 'size':22, 'type':132 }
        ]";
    public static readonly List<string> Summary = new List<string>();
    public static ConnectionStatus connectionStatus { get; set; }
    public static byte[] datablock2Bytes { get; set; }
    public static List<DataBlocks> datablockBytes { get; set; }
    private static Plc _client = new Plc(CpuType.S71500,"127.0.0.1", 0,1);
    static int _ping_ok = 2;
   private async Task MonitorConnection(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    if (_ping_ok == 0)
                    {
                        _ping_ok = 2;
                    }
                    if (_ping_ok == 1)
                    {
                        Console.WriteLine("PING OK");
                        Connection_Start1();
                        _ping_ok = 0;
                    }
                    else if (_ping_ok == 2)
                    {
                        Console.WriteLine("PING FAILURE!");

                        Ping(_client.IP, 1, 500, 1);
                    }
                }

            }
            catch
            {

            }
            await Task.Delay(2000, stoppingToken); // Example delay
        }
    }
    private async Task GatheringData(CancellationToken stoppingToken)
    {
      
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_client.IsConnected)
            {
                connectionStatus = (ConnectionStatus)2;
                try
                {
                    //datablock2Bytes = _client.ReadBytes(DataType.DataBlock, 5, 0, 8);
                    foreach(var item in datablockBytes)
                    {
                        var temp = a.Where(x => x.ID == item.ConfigID).First();
                        item.bytes =  _client.ReadBytes((S7.Net.DataType)temp.type, temp.db, 0, temp.size);
                    }
                }
                catch
                {

                }
            }
            else
            {
                connectionStatus = (ConnectionStatus)1;
            }
            await Task.Delay(200, stoppingToken); // Example delay
        }
    }
    List<BytesConfig> a;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        a = JsonConvert.DeserializeObject<List<BytesConfig>>(jsonStringBytesConfig);
        datablockBytes = new List<DataBlocks>();
        foreach (BytesConfig item in a)
        {
            datablockBytes.Add(new DataBlocks()
            {
                ConfigID = item.ID,
            
            });
        }
        var task1 = Task.Run(() => MonitorConnection(stoppingToken), stoppingToken);
        var task2 = Task.Run(() => GatheringData(stoppingToken), stoppingToken);

        await Task.WhenAll(task1, task2);
    }
    #region Ping Operations
    public void Ping(string host, int attempts, int timeout, int plc)
    {
        for (int i = 0; i < attempts; i++)
        {
            new Thread(delegate ()
            {
                try
                {
                    Ping ping = new Ping();
                    switch (plc)
                    {
                        case 1:
                            ping.PingCompleted += new PingCompletedEventHandler(PingCompleted1);
                            break;
                        default:
                            break;
                    }
                    ping.SendAsync(host, timeout, host);
                }
                catch { }
            }).Start();
        }
    }

    private void PingCompleted1(object sender, PingCompletedEventArgs e)
    {
        string ip = (string)e.UserState;
        if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        {
            _ping_ok = 1;
        }
        else _ping_ok = 2;
    }
    private async void Connection_Start1()
    {
        await Conncetion_Start11();
    }
    Task Conncetion_Start11()
    {
        return Task.Run(() =>
        {
            try
            {
                _client.Open();
                Console.WriteLine("PLC is Connected");
            }
            catch (Exception)
            {
            }
        });
    }
    #endregion
}