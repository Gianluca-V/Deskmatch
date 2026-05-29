param([string]$Service = "")

$REMOTE_HOST = "192.168.1.58"
$REMOTE_USER = "root"

if ($Service) {
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "docker logs --tail 50 deskmatch-$Service"
} else {
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "cd /srv/deskmatch/infrastructure/docker && docker compose logs --tail 20"
}