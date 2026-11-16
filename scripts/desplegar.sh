#!/usr/bin/env bash
# Despliega (o actualiza) la aplicación en este servidor.
# Uso: ./scripts/desplegar.sh
set -euo pipefail
cd "$(dirname "$0")/.."

if [ ! -f .env ]; then
  echo "Falta el archivo .env. Créalo con: ./scripts/crear-env.sh http://IP_PUBLICA"
  exit 1
fi

# Si es un repositorio Git con remoto, trae los últimos cambios.
if git rev-parse --is-inside-work-tree >/dev/null 2>&1 && git remote | grep -q .; then
  echo "Actualizando el código..."
  git pull --ff-only
fi

echo "Construyendo y levantando los contenedores..."
docker compose up -d --build

echo
docker compose ps
echo
./scripts/verificar.sh
echo
PUERTO="$(grep -E '^PUERTO_WEB=' .env | cut -d= -f2)"
ORIGEN="$(grep -E '^ORIGEN_PUBLICO=' .env | cut -d= -f2)"
echo "Listo. La primera vez la base tarda ~1 minuto en crearse."
echo "Abre: ${ORIGEN}$([ "${PUERTO:-80}" != "80" ] && echo ":${PUERTO}")"
