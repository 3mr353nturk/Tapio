# Tapio web (Vite 5173) — API /api ve /uploads proxy üzerinden gider.
# Önkoşul: ngrok kurulu, docker + API (5045) + npm run dev (5173) çalışıyor olmalı.
#
# 1) ngrok http 5173
# 2) Verilen https URL'yi src/Tapio.Web/.env.local içine yazın:
#    VITE_PUBLIC_BASE_URL=https://XXXX.ngrok-free.app
# 3) Vite'ı yeniden başlatın; panelde Paylaşım → QR linki güncellenir.

Write-Host "Ngrok baslatiliyor: http 5173 ..."
Write-Host "API yerelde 5045'te calismali (Vite proxy)."
ngrok http 5173
