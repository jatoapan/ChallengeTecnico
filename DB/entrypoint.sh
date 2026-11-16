#!/bin/bash
# Arranca SQL Server y, la primera vez, crea la base con createDB.sql.
# createDB.sql no se puede repetir (crea las tablas), así que solo se ejecuta si la base no existe:
# al reiniciar el contenedor con el volumen de datos, la base y sus datos se conservan.
set -e

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"   # sqlcmd lee la clave de aquí: no aparece en la lista de procesos

/opt/mssql/bin/sqlservr &
SQL_PID=$!

# Al detener el contenedor, apagar SQL Server de forma ordenada.
trap 'kill -TERM "$SQL_PID"; wait "$SQL_PID"' TERM INT

echo "Esperando a que SQL Server acepte conexiones..."
for _ in $(seq 1 60); do
  if $SQLCMD -S localhost -U sa -C -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

EXISTE=$($SQLCMD -S localhost -U sa -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'InventarioProductos'" | tr -d '[:space:]')

if [ "$EXISTE" = "0" ]; then
  echo "Creando la base InventarioProductos con sus datos iniciales..."
  $SQLCMD -S localhost -U sa -C -b -i /usr/src/app/createDB.sql
  echo "Base creada."
else
  echo "La base InventarioProductos ya existe: se conservan sus datos."
fi

wait "$SQL_PID"
