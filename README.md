# Tapio MVP

NFC + QR stand üzerinden işletme dijital sayfasına yönlendiren SaaS platformu.

## Gereksinimler

- .NET 8 SDK
- Node.js 20+
- Docker (SQL Server + Redis)

## Başlangıç

```bash
# Altyapı
docker compose -f docker/docker-compose.yml up -d

# Backend
dotnet ef migrations add InitialCreate -p src/Tapio.Infrastructure -s src/Tapio.API
dotnet run --project src/Tapio.API

# Frontend
cd src/Tapio.Web
npm install
npm run dev
```

## Varsayılan Admin

- E-posta: `admin@tapio.app`
- Şifre: `Admin123!`

## URL'ler

- API: http://localhost:5045 (launch profile `http`)
- Web: http://localhost:5173
- Public page (kısa yol): `/t/{slug}` — isteğe bağlı stand: `/t/{slug}?s={standCode}`
- Menü örneği: `/menus/kafe.html`

## İşletme paneli — Paylaşım

- **Paylaşım** veya **Genel Bakış**: tam public URL, kısa yol, QR **PNG/SVG** indirme.
- **QR ve NFC** aynı tam URL olmalıdır.

## ngrok ile dışarı açma (demo)

1. Docker + API (`dotnet run --project src/Tapio.API --launch-profile http`) + Web (`npm run dev`).
2. `ngrok http 5173` veya `powershell -File scripts/ngrok-web.ps1`
3. `src/Tapio.Web/.env.local` oluşturun (`.env.example` referans):

   `VITE_PUBLIC_BASE_URL=https://SIZIN-ID.ngrok-free.app`

4. Vite’ı yeniden başlatın; panelde QR linki ngrok adresini kullanır.

API CORS, `localhost` ve `ngrok` origin’lerine izin verecek şekilde ayarlıdır.

## Oturum süreleri (varsayılan)

- Access token: **480 dk** (~8 saat) — `Jwt:AccessTokenMinutes`
- Refresh token: **30 gün** — `Jwt:RefreshTokenDays` (cookie ile uyumlu)
