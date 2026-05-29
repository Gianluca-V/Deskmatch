# DeskMatch - Push to remote dev server and deploy
# Usage: .\devops\push.ps1 [-Full]
#
# Without flags: rsync only changed files, then docker compose up -d
# -Full: clean rebuild (docker compose build --no-cache + up -d)

param([switch]$Full)

$ErrorActionPreference = "Stop"

$REMOTE_HOST = "192.168.1.58"
$REMOTE_USER = "root"
$REMOTE_PATH = "/srv/deskmatch"
$LOCAL_PATH = Resolve-Path "$PSScriptRoot\.."

Write-Host "=== DeskMatch Deploy ===" -ForegroundColor Cyan
Write-Host "Target: ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_PATH}" -ForegroundColor Gray

Write-Host "`n[1/3] Creating remote directories..." -ForegroundColor Yellow
ssh "${REMOTE_USER}@${REMOTE_HOST}" "mkdir -p ${REMOTE_PATH}/apps ${REMOTE_PATH}/shared ${REMOTE_PATH}/infrastructure ${REMOTE_PATH}/devops"

Write-Host "[2/3] Syncing files (rsync)..." -ForegroundColor Yellow
$excludes = @(
    ".git",
    "node_modules",
    ".nuget",
    "bin",
    "obj",
    "*.user",
    "*.suo",
    ".vs",
    ".idea",
    ".vscode",
    "dist",
    "logs"
)

$excludeArgs = $excludes | ForEach-Object { "--exclude=`"$_`"" }
$excludeStr = $excludeArgs -join " "

$rsyncCmd = "rsync -avz --delete $excludeStr -e ssh `"$LOCAL_PATH/`" ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_PATH}/"
Write-Host "  $rsyncCmd" -ForegroundColor DarkGray

Invoke-Expression $rsyncCmd

if ($Full) {
    Write-Host "`n[3/3] Full rebuild + deploy..." -ForegroundColor Yellow
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "cd ${REMOTE_PATH}/infrastructure/docker && docker compose down && docker compose build --no-cache && docker compose up -d"
} else {
    Write-Host "`n[3/3] Fast deploy (docker compose up -d)..." -ForegroundColor Yellow
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "cd ${REMOTE_PATH}/infrastructure/docker && docker compose up -d --build"
}

Write-Host "`n=== Done ===" -ForegroundColor Green
Write-Host "Services:" -ForegroundColor Cyan
ssh "${REMOTE_USER}@${REMOTE_HOST}" "docker ps --filter name=deskmatch --format 'table {{.Names}}\t{{.Status}}'"