#!/bin/sh
set -eu
existe=$(psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres -tAc "SELECT 1 FROM pg_database WHERE datname = 'crialab_api'")
if [ "$existe" != "1" ]; then
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres -c "CREATE DATABASE crialab_api"
fi
