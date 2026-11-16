export interface LoginRequest {
  usuario: string;
  password: string;
}

export interface LoginRespuesta {
  token: string;
  usuario: string;
  rol: string;
  expira: string;
}