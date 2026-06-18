# Observability.OpenTelemetry

OpenTelemetry öğrenme sürecimde; distributed tracing, metrics ve observability kavramlarını .NET projeleri üzerinde uyguladığım örnek repository.

## Amaç

Bu proje, bir uygulamanın içinde gerçekleşen işlemleri daha görünür hale getirmek için OpenTelemetry kullanımını göstermeyi amaçlar.

Projede temel olarak şunlar uygulanmıştır:

- Console uygulamasında manuel tracing
- ASP.NET Core Web API üzerinde request tracing
- HTTP Client tracing
- Entity Framework Core tracing
- Custom tag ve event kullanımı
- Request/response body bilgisini trace içerisine ekleme
- Metrics API ile custom metric üretimi
- Console, OTLP ve Prometheus exporter kullanımı

## Proje Yapısı

```text
Observability.OpenTelemetry
│
├── Observability.OpenTelemetry.ConsoleApp
│   └── Manuel ActivitySource ve tracing örneği
│
├── Observability.OpenTelemetry.Tracing.WebAPI
│   └── ASP.NET Core, EF Core, HTTP Client ve custom middleware tracing örneği
│
└── Observability.OpenTelemetry.Metrics.WebAPI
    └── Custom metric ve Prometheus exporter örneği
