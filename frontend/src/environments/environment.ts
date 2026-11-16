// Producción (ng build usa este archivo). La API se pide por la misma dirección del sitio: Nginx
// reenvía /api a la API, así el front no necesita conocer su dirección y no hay problemas de CORS.
export const environment = {
  production: true,
  apiUrl: '/api'
};
