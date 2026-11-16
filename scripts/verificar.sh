#!/usr/bin/env bash
# Comprueba que el sitio y la API responden después de un despliegue. Termina con error si no.
# Se usa desde desplegar.sh, así el pipeline de GitHub falla si la aplicación no levantó.
#  1) Por el puerto local de Nginx: el front responde 200 y la API 401 (exige sesión).
#  2) Si hay un dominio real: el HTTPS público responde con un certificado válido (Caddy).
set -euo pipefail
cd "$(dirname "$0")/.."

leer() { grep -E "^$1=" .env 2>/dev/null | cut -d= -f2- || true; }

PUERTO="$(leer PUERTO_LOCAL)"; PUERTO="${PUERTO:-8081}"
DOMINIO="$(leer DOMINIO)"; DOMINIO="${DOMINIO:-localhost}"
BASE="http://localhost:${PUERTO}"

# La primera vez la base tarda ~1 minuto en crearse: se reintenta hasta 3 minutos.
for intento in $(seq 1 36); do
  SITIO="$(curl -s -o /dev/null -w '%{http_code}' "${BASE}/" || true)"
  API="$(curl -s -o /dev/null -w '%{http_code}' "${BASE}/api/Lotes" || true)"
  if [ "$SITIO" = "200" ] && [ "$API" = "401" ]; then
    echo "Aplicación correcta: sitio 200 y API 401 (exige sesión)."
    break
  fi
  echo "Esperando a la aplicación (${intento}/36): sitio=${SITIO} api=${API}"
  sleep 5
  if [ "$intento" = "36" ]; then
    echo "La aplicación no respondió bien tras 3 minutos. Últimos logs de la API:"
    docker compose logs --tail=30 api || true
    exit 1
  fi
done

if [ "$DOMINIO" = "localhost" ]; then
  echo "Dominio localhost: no se verifica el HTTPS público."
  exit 0
fi

# HTTPS público: se resuelve el dominio a esta misma máquina y NO se ignora el certificado,
# así se comprueba que el certificado de Let's Encrypt es válido. Caddy tarda unos segundos en obtenerlo.
for intento in $(seq 1 24); do
  CODIGO="$(curl -s -o /dev/null -w '%{http_code}' --resolve "${DOMINIO}:443:127.0.0.1" "https://${DOMINIO}/" || true)"
  if [ "$CODIGO" = "200" ]; then
    echo "HTTPS correcto: https://${DOMINIO} responde 200 con certificado válido."
    exit 0
  fi
  echo "Esperando el certificado HTTPS (${intento}/24): código=${CODIGO}"
  sleep 5
done

echo "No se pudo obtener o usar el certificado HTTPS de ${DOMINIO}. Comprueba:"
echo "  - que el DNS de ${DOMINIO} apunte a la IP de este servidor"
echo "  - que el grupo de seguridad permita los puertos 80 y 443 desde internet"
docker compose logs --tail=30 caddy || true
exit 1
