#!/usr/bin/env bash
# Crea el archivo .env con contraseñas y clave JWT aleatorias.
# Uso: ./scripts/crear-env.sh http://IP_O_DOMINIO_PUBLICO [true|false]
#      (el segundo parámetro activa o no Swagger; por defecto true para la revisión)
set -euo pipefail
cd "$(dirname "$0")/.."

if [ -f .env ]; then
  echo "Ya existe un archivo .env. No lo sobrescribo (bórralo si quieres generarlo de nuevo)."
  exit 1
fi

ORIGEN="${1:-http://localhost}"
SWAGGER="${2:-true}"

# hex = solo letras y números, sin caracteres que rompan la cadena de conexión.
# El sufijo "Aa1!" asegura la complejidad que exige SQL Server.
CLAVE_SQL="$(openssl rand -hex 16)Aa1!"
CLAVE_JWT="$(openssl rand -hex 32)"

umask 077   # el .env queda legible solo por su dueño
cat > .env <<EOF
MSSQL_SA_PASSWORD=${CLAVE_SQL}
JWT_CLAVE=${CLAVE_JWT}
ORIGEN_PUBLICO=${ORIGEN}
SWAGGER_HABILITADO=${SWAGGER}
PUERTO_WEB=80
PUERTO_SQL=1433
EOF

echo ".env creado con claves aleatorias (origen: ${ORIGEN}, Swagger: ${SWAGGER})."
echo "Guarda una copia de MSSQL_SA_PASSWORD en un lugar seguro: sin ella no se puede entrar a la base."
