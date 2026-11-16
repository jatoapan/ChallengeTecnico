#!/usr/bin/env bash
# Comprueba que el sitio y la API responden después de un despliegue. Termina con error si no.
# Se usa desde desplegar.sh, así el pipeline de GitHub falla si la aplicación no levantó.
# Uso: ./scripts/verificar.sh [puerto]   (por defecto el de PUERTO_WEB del .env, o 80)
set -euo pipefail
cd "$(dirname "$0")/.."

PUERTO="${1:-$(grep -E '^PUERTO_WEB=' .env 2>/dev/null | cut -d= -f2 || true)}"
PUERTO="${PUERTO:-80}"
BASE="http://localhost:${PUERTO}"

# La primera vez la base tarda ~1 minuto en crearse: se reintenta hasta 3 minutos.
for intento in $(seq 1 36); do
  SITIO="$(curl -s -o /dev/null -w '%{http_code}' "${BASE}/" || true)"
  # Sin token la API debe responder 401: eso prueba que Nginx llega a la API y la API está viva.
  API="$(curl -s -o /dev/null -w '%{http_code}' "${BASE}/api/Lotes" || true)"
  if [ "$SITIO" = "200" ] && [ "$API" = "401" ]; then
    echo "Verificación correcta: sitio 200 y API 401 (exige sesión) en ${BASE}"
    exit 0
  fi
  echo "Esperando a la aplicación (${intento}/36): sitio=${SITIO} api=${API}"
  sleep 5
done

echo "La aplicación no respondió bien tras 3 minutos. Últimos logs de la API:"
docker compose logs --tail=30 api || true
exit 1
