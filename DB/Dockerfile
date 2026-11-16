# SQL Server 2022 con la base de Inventario: se crea sola la primera vez que arranca el contenedor.
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
WORKDIR /usr/src/app
COPY createDB.sql entrypoint.sh ./
RUN chmod +x entrypoint.sh

USER mssql
ENTRYPOINT ["/usr/src/app/entrypoint.sh"]
