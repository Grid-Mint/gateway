# Gateway

API Gateway на базі [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/), який проксує запити до внутрішніх сервісів (users, і т.д.).

## Як це працює

Уся логіка в [Program.cs](src/Gateway/Program.cs) зводиться до трьох рядків:

```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
...
app.MapReverseProxy();
```

YARP читає секцію `ReverseProxy` з [appsettings.json](src/Gateway/appsettings.json) і будує з неї проксі. Секція складається з двох частин:

- **Routes** — правила, за якими вхідний запит (шлях, метод, хости тощо) прив'язується до конкретного кластера.
- **Clusters** — куди фактично йде проксований запит (одна або декілька destinations — бекенд-сервісів).

Приклад поточного конфіга:

```json
"ReverseProxy": {
  "Routes": {
    "users": {
      "ClusterId": "users",
      "Match": {
        "Path": "/users/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "users": {
      "Destinations": {
        "d1": {
          "Address": "http://lacarte-users-api:8080/"
        }
      }
    }
  }
}
```

Це означає: будь-який запит на `/users/*` (наприклад `/users/123`) буде проксований на `http://lacarte-users-api:8080/123`. Ключі `"users"` в `Routes` і `Clusters` — це просто ідентифікатори, `ClusterId` в роуті прив'язує їх один до одного.

## Як додати новий маршрут / сервіс

Щоб додати проксування нового сервісу (наприклад `orders`), треба додати одночасно роут і кластер в `appsettings.json`:

```json
"ReverseProxy": {
  "Routes": {
    "users": { ... },
    "orders": {
      "ClusterId": "orders",
      "Match": {
        "Path": "/orders/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "users": { ... },
    "orders": {
      "Destinations": {
        "d1": {
          "Address": "http://lacarte-orders-api:8080/"
        }
      }
    }
  }
}
```

Ключові моменти:

- `Match.Path` з `{**catch-all}` в кінці — щоб проксувати весь шлях "як є", включно з вкладеними сегментами.
- `ClusterId` в роуті має точно збігатися з ключем відповідного кластера в `Clusters`.
- `Destinations` може містити декілька адрес (`d1`, `d2`, ...) — тоді YARP буде балансувати навантаження між ними (round-robin за замовчуванням).
- Адреса destination має закінчуватись `/`.

Перезапускати код не потрібно — секція `ReverseProxy` читається з конфігурації, тому YARP підтримує live-reload при зміні `appsettings.json` (якщо увімкнено reloadOnChange, що є за замовчуванням в ASP.NET Core).

## Розширені можливості (за потреби)

В YARP можна також налаштовувати (додаванням відповідних полів у `Routes`/`Clusters`):

- **Match.Methods** — обмежити роут конкретними HTTP-методами (`GET`, `POST`, ...).
- **Match.Hosts** — маршрутизація за хостом.
- **Transforms** — трансформація шляху/заголовків перед проксуванням (наприклад, обрізати префікс `/users`).
- **HealthCheck** — активні/пасивні health-check'и для destinations в кластері.
- **LoadBalancingPolicy** в кластері — стратегія балансування (`RoundRobin`, `LeastRequests`, `PowerOfTwoChoices`, `Random`, `First`).

Детальніше — офіційна документація: https://microsoft.github.io/reverse-proxy/articles/config-files.html

## Секція Services

Секція `Services` в `appsettings.json` (наприклад `Services:Users:BaseUrl`) наразі не використовується YARP напряму — це окремий конфіг для випадків, коли потрібно звертатись до сервісу напряму з коду (не через проксі), а не частина `ReverseProxy` конфігурації.
