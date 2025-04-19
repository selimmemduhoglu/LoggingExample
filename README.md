# Logging ve Exception Handling Örnek Projesi

Bu proje, modern .NET uygulamalarında yapılandırılmış logging ve kapsamlı exception handling implementasyonunu göstermektedir.

## Özellikler

- **Kapsamlı Exception Handling**
  - Global exception middleware
  - Özelleştirilmiş exception tipleri
  - HTTP durum kodlarına uygun hata yanıtları
  - İstemci dostu hata mesajları
  - Detaylı hata loglaması
  - Korelasyon ID ile hata izleme

- **Yapılandırılmış Logging**
  - Standart API yanıt formatı
  - Korelasyon ID ile request-response takibi
  - Elastic Stack entegrasyonu (Elasticsearch, Kibana)
  - Seq entegrasyonu
  - OpenTelemetry desteği
  - Prometheus ve Jaeger ile izleme

- **Validation**
  - Model validasyonu ve doğrulama
  - Tutarlı hata yanıtları
  - Yapılandırılmış validation mesajları

## Exception Tipleri

- `ApiException`: Tüm özel istisnaların temel sınıfı
- `NotFoundException`: 404 Not Found
- `BadRequestException`: 400 Bad Request
- `UnauthorizedException`: 401 Unauthorized
- `ForbiddenException`: 403 Forbidden
- `ValidationException`: 422 Unprocessable Entity
- `BusinessException`: 409 Conflict
- `ExternalServiceException`: 502 Bad Gateway

## Proje Yapısı

- **Controllers**: API endpoint'leri
- **Middleware**: Request/response ve exception işleme
- **Models**: Veri modelleri ve DTO'lar
- **Models/Exceptions**: Özel exception sınıfları
- **Extensions**: Extension metodları
- **Filters**: Controller filtreleri (validation vb.)
- **Configurations**: Uygulama konfigürasyonları

## Kullanım

API üzerinden exception testi yapmak için şu endpoint'leri kullanabilirsiniz:

- `GET /api/ExceptionTest/success`: Başarılı yanıt döndürür
- `GET /api/ExceptionTest/not-found`: 404 hatası
- `GET /api/ExceptionTest/bad-request`: 400 hatası
- `GET /api/ExceptionTest/unauthorized`: 401 hatası
- `GET /api/ExceptionTest/forbidden`: 403 hatası
- `GET /api/ExceptionTest/validation`: 422 hatası
- `GET /api/ExceptionTest/business`: 409 hatası
- `GET /api/ExceptionTest/external-service`: 502 hatası
- `GET /api/ExceptionTest/server-error`: 500 hatası
- `GET /api/ExceptionTest/divide-by-zero`: 500 hatası (sıfıra bölme)
- `GET /api/ExceptionTest/null-reference`: 500 hatası (null referans)

API üzerinden validasyon testi yapmak için kullanıcı API'sini kullanabilirsiniz:

- `GET /api/Users`: Tüm kullanıcıları listeler
- `GET /api/Users/{id}`: ID'ye göre kullanıcı getirir
- `POST /api/Users`: Yeni kullanıcı oluşturur (validasyon test edilebilir)
- `PUT /api/Users/{id}`: Kullanıcı günceller
- `DELETE /api/Users/{id}`: Kullanıcı siler

## Docker Ortamında Çalıştırma

```bash
docker-compose up -d
```

Servisler:
- Web API: http://localhost:5000
- Elasticsearch: http://localhost:9200
- Kibana: http://localhost:5601
- Seq: http://localhost:5341
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090 