#!/usr/bin/env bash
# Prepara una instancia EC2 con Ubuntu 24.04: instala Docker y crea memoria de intercambio (swap).
# Uso (una sola vez, dentro de la EC2): sudo bash scripts/preparar-ec2.sh
set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "Ejecútalo con sudo: sudo bash scripts/preparar-ec2.sh"
  exit 1
fi

apt-get update -y
apt-get install -y docker.io docker-compose-v2 git openssl
systemctl enable --now docker

# Permite usar docker sin sudo al usuario que inició sesión (aplica al volver a entrar por SSH).
USUARIO="${SUDO_USER:-ubuntu}"
usermod -aG docker "$USUARIO"

# SQL Server pide bastante memoria: 2 GB de swap evitan que el sistema lo mate en instancias pequeñas.
if ! swapon --show | grep -q .; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  echo "/swapfile none swap sw 0 0" >> /etc/fstab
  echo "Swap de 2 GB creado."
fi

echo
echo "Listo. Cierra la sesión SSH y vuelve a entrar para usar docker sin sudo."
