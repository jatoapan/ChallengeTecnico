# Etapa 1: compilar Angular en modo producción
FROM node:24-alpine AS build
WORKDIR /app

# Primero las dependencias: Docker reutiliza esta capa mientras package*.json no cambie.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npx ng build --configuration production

# Etapa 2: Nginx sirve los archivos estáticos y reenvía /api a la API
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
EXPOSE 80
