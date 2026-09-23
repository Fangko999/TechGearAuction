import { IAuthRepository } from '@domain/repositories/IAuthRepository';
import { LoginRequest, AuthResponse, RegisterRequest } from '@domain/models/auth';
import axiosClient from '../api/axiosClient';

export class AuthRepository implements IAuthRepository {
  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await axiosClient.post<any, AuthResponse>('/auth/login', request);
    return response;
  }

  async register(request: RegisterRequest): Promise<void> {
    await axiosClient.post('/auth/register', request);
  }
}

