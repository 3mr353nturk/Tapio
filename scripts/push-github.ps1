# GitHub'a ilk push (önce: gh auth login)
# Repo adi farkli olacaksa: gh repo create YOUR-REPO-NAME --public ...

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
  Write-Host "GitHub CLI yok. winget install GitHub.cli"
  exit 1
}

gh auth status 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Host "Once giris yapin: gh auth login"
  exit 1
}

$repoName = if ($args[0]) { $args[0] } else { "Tapio" }
gh repo create $repoName --public --source=. --remote=origin --push --description "Tapio MVP - NFC/QR isletme sayfasi"

Write-Host "Tamam. GitHub'da repoyu kontrol edin."
