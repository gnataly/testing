import axios from 'axios';
import { AuthResponse, AccountDTO } from '../types';

const API_URL = 'https://localhost:5001/api/Accounts';

// Хэширование пароля на клиенте перед отправкой
const hashPassword = async (password: string): Promise<string> => {
  // В реальном приложении используйте более надежное хэширование
  const encoder = new TextEncoder();
  const data = encoder.encode(password);
  const hashBuffer = await crypto.subtle.digest('SHA-256', data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
};

export const login = async (username: string, password: string): Promise<AuthResponse> => {
  const passwordHash = await hashPassword(password);
  const response = await axios.post<AuthResponse>(`${API_URL}/auth/login`, {
    username,
    passwordHash
  });
  return response.data;
};

export const register = async (username: string, password: string): Promise<AuthResponse> => {
  const passwordHash = await hashPassword(password);
  try {
    const response = await axios.post<AuthResponse>(
      `${API_URL}/auth/register`,
      { 
        Username: username,
        PasswordHash: passwordHash
      },
      {
        headers: {
          'Content-Type': 'application/json',
        }
      }
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error('Registration error:', error.response?.data);
      throw new Error(error.response?.data || 'Registration failed');
    }
    throw error;
  }
};

export const getCurrentUser = async (): Promise<AccountDTO | null> => {
  try {
    const response = await axios.get<AccountDTO>(`${API_URL}/me`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    return response.data;
  } catch (error) {
    console.error('Failed to fetch current user:', error);
    return null;
  }
};

// export const updateAccount = async (id: number, accountData: Partial<AccountDTO>): Promise<AccountDTO> => {
//   const response = await axios.put<AccountDTO>(`${API_URL}/${id}`, accountData, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
//   return response.data;
// };

// export const deleteAccount = async (id: number): Promise<void> => {
//   await axios.delete(`${API_URL}/${id}`, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
// };

// // Функции для работы с избранным
// export const addFavoriteActor = async (accountId: number, actorId: number): Promise<void> => {
//   await axios.post(`${API_URL}/${accountId}/favorites/actors/${actorId}`, null, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
// };

// export const removeFavoriteActor = async (accountId: number, actorId: number): Promise<void> => {
//   await axios.delete(`${API_URL}/${accountId}/favorites/actors/${actorId}`, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
// };

// // Аналогичные функции для musicals и theatres
// export const addFavoriteMusical = async (accountId: number, musicalId: number): Promise<void> => {
//   await axios.post(`${API_URL}/${accountId}/favorites/musicals/${musicalId}`, null, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
// };

// export const removeFavoriteMusical = async (accountId: number, musicalId: number): Promise<void> => {
//   await axios.delete(`${API_URL}/${accountId}/favorites/musicals/${musicalId}`, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
// };

// export const getFavorites = async (accountId: number): Promise<{
//   favoriteActors: FavoriteItemDTO[];
//   favoriteMusicals: FavoriteItemDTO[];
//   favoriteTheatres: FavoriteItemDTO[];
// }> => {
//   const response = await axios.get(`${API_URL}/${accountId}/favorites`, {
//     headers: {
//       Authorization: `Bearer ${localStorage.getItem('token')}`
//     }
//   });
//   return response.data;
// };