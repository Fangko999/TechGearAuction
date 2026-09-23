export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  displayName: string;
  role: string;
  token: string;
}


export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}
