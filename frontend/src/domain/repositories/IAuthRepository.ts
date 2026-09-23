import { LoginRequest, AuthResponse, RegisterRequest } from '../models/auth';

export interface IAuthRepository {
  login(request: LoginRequest): Promise<AuthResponse>;
  register(request: RegisterRequest): Promise<void>;
}

