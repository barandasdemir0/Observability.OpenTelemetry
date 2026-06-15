using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Observability.OpenTelemetry.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        //her zaman using kullanılmak zorunda
        using TracerProvider tracerProvider = Sdk
            .CreateTracerProviderBuilder()
            .AddSource("ConsoleApp.Trace")//takip edebileceğim source
            .ConfigureResource(configure =>
            {
                configure.AddService("MyConsoleService"); //trace e spesifik bir ad ekledim
                configure.AddAttributes(
                    new List<KeyValuePair<string, object>>()
                    {
                        new("test key","test value"),
                        new("machine.name",Environment.MachineName),
                        new("os.version",Environment.OSVersion.ToString()),
                        new("process.path",Environment.ProcessPath ?? "")//nullsa boş gelmesini söyledim
                    });//trace attribute ekledim
            })//service adı unknown yerine isim yazmak için kullanırız
            .AddConsoleExporter()//gösterme ekranı console
            .AddOtlpExporter()//jeager bağlamak için 16686 da bağlandığmız direkt işler başka portsa içine configure ile gir hallet
            .Build();

        ServiceHelper service = new();
        service.Method1();

        Console.ReadLine();
    }
}

internal class ServiceHelper
{
    public void Method1()
    {
        //using (var BlockaActivity = ActivitySourceProvider.ActivitySource.StartActivity())
        //{
        //    Console.WriteLine("Process1");
        //    Console.WriteLine("Process2");
        //    //sadece bu ikisini takip eder
        //};

        //takip edeceğim aktivcity başlatıyor ve altında ne yaparsam takip edebilirim yani üstteki belirli alanken alttaki hepsi
        using var activity = ActivitySourceProvider.ActivitySource.StartActivity()!;
        activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;
        activity.AddTag("user.id", "1"); //tag ekler
        activity.AddTag("user.id", "2"); //tag ekler
        activity.SetTag("user.id", "3"); //varsa değiştir 1.yi ezer 2.yi ezemez

        
        Console.WriteLine("Process 1");
        Console.WriteLine("Process 2");
        activity.AddEvent(new ActivityEvent("my.event"));
        Console.WriteLine("Process 3");

       

        Method2();
    }

    public void Method2()
    {
        using var activity = ActivitySourceProvider.ActivitySource.StartActivity();
        Console.WriteLine("Process 4");
    }
}


internal static class ActivitySourceProvider
{
    public static ActivitySource ActivitySource = new("ConsoleApp.Trace", "1.0.0");
}

/*
    zipkin opentelemetry ve jaeger opentelemetry bunlar görselleştirme araçları free


Activity.TraceId
Bu isteğin/akışın genel kimliği. Aynı işlem zincirindeki tüm span’lar aynı TraceId değerini paylaşır.

Activity.SpanId
Bu spesifik işlemin kimliği. Burada Method1 için oluşan span’ın ID’si.

Activity.TraceFlags: Recorded
Bu span kaydedilmiş demek. Yani exporter bunu dışarı gönderebilir veya console’a yazabilir.

Activity.DisplayName: Method1
Span’ın adı.
activitySource.StartActivity("Method1");

Activity.Kind: Internal
Bu span uygulamanın kendi iç işlemi demek. HTTP client/server gibi dış çağrı değil.

Activity.StartTime
Span’ın başladığı zaman. UTC formatında.

Activity.Duration
Span’ın ne kadar sürdüğü. Burada yaklaşık 6 ms.

Instrumentation scope
Bu span’ı hangi ActivitySource ürettiğini gösterir.

Name: ConsoleApp.Trace
Version: 1.0.0
Yani senin trace kaynağın ConsoleApp.Trace, versiyonu da 1.0.0.

Resource associated with Activity
Bu span’ın hangi uygulama/makine/process tarafından üretildiğini anlatan metadata kısmı.

service.name: MyConsoleService
Bu uygulamanın servis adı.

machine.name: BARANPC
Çalıştığı bilgisayar adı.

os.version: Microsoft Windows NT 10.0.26200.0
İşletim sistemi versiyonu.

process.path: ...
Çalışan .exe dosyasının yolu.

service.instance.id: abd05a83...
Bu uygulama instance’ına verilen benzersiz ID. Uygulama her çalıştığında değişebilir.

telemetry.sdk.name: opentelemetry
telemetry.sdk.language: dotnet
telemetry.sdk.version: 1.16.0

Bu trace’i üreten SDK bilgisi. Yani .NET OpenTelemetry SDK 1.16.0 kullanılmış.

TraceId  -> bütün işlem zinciri
SpanId   -> tek bir işlem
Method1  -> izlenen metodun adı
Duration -> metodun çalışma süresi
Resource -> bu trace hangi servis/makineden geldi

Senin örnekte OpenTelemetry şunu söylüyor:

“MyConsoleService adlı console app içinde Method1 isimli işlem çalıştı, yaklaşık 6 ms sürdü, BARANPC makinesinde çalışıyordu ve bu işlem trace olarak kaydedildi.”

*/