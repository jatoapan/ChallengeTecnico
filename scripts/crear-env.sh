#!/usr/bin/env bash
# Crea el archivo .env con contraseñas y clave JWT aleatorias.
# Uso: ./scripts/crear-env.sh DOMINIO [true|false]
#   DOMINIO: el nombre público del sitio, por ejemplo inventario-jatoapan.duckdns.org
#            (use "localhost" para probar en su máquina). El segundo parámetro activa o no Swagger.
set -euo pipefail
cd "$(dirname "$0")/.."

if [ -f .env ]; then
  echo "Ya existe un archivo .env. No lo sobrescribo (bórralo si quieres generarlo de nuevo)."
  exit 1
fi

DOMINIO="${1:-localhost}"
SWAGGER="${2:-true}"

# En un dominio real el sitio va por HTTPS; en localhost se prueba por HTTP.
if [ "$DOMINIO" = "localhost" ]; then ORIGEN="http://localhost"; else ORIGEN="https://${DOMINIO}"; fi

# hex = solo letras y números, sin caracteres que rompan la cadena de conexión.
# El sufijo "Aa1!" asegura la complejidad que exige SQL Server.
CLAVE_SQL="$(openssl rand -hex 16)Aa1!"
CLAVE_JWT="$(openssl rand -hex 32)"

umask 077   # el .env queda legible solo por su dueño
cat > .env <<EOF2
MSSQL_SA_PASSWORD=${CLAVE_SQL}
JWT_CLAVE=${CLAVE_JWT}
DOMINIO=${DOMINIO}
ORIGEN_PUBLICO=${ORIGEN}
SWAGGER_HABILITADO=${SWAGGER}
EOF2

echo ".env creado con claves aleatorias (dominio: ${DOMINIO}, Swagger: ${SWAGGER})."
echo "Guarda una copia de MSSQL_SA_PASSWORD en un lugar seguro: sin ella no se puede entrar a la base."
