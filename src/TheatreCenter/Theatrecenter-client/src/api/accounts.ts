// api/accounts.ts
import axios from 'axios';
import { AccountDTO, AccountFavoritesDTO, AddFavoriteDTO, RemoveFavoriteDTO } from '../types';

const API_URL = 'https://localhost:5001/api/Accounts';

export const getAccountById = async (id: number): Promise<AccountDTO> => {
  const response = await axios.get<AccountDTO>(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const getAccountFavorites = async (accountId: number): Promise<AccountFavoritesDTO> => {
  const response = await axios.get<AccountFavoritesDTO>(`${API_URL}/${accountId}/favorites`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const addFavoriteActor = async (data: AddFavoriteDTO): Promise<void> => {
  await axios.post(`${API_URL}/${data.accountId}/favorites/actors/${data.targetId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      }
    });
};

export const removeFavoriteActor = async (data: RemoveFavoriteDTO): Promise<void> => {
  await axios.delete(`${API_URL}/${data.accountId}/favorites/actors/${data.targetId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const addFavoriteMusical = async (data: AddFavoriteDTO): Promise<void> => {
  await axios.post(`${API_URL}/${data.accountId}/favorites/musicals/${data.targetId}`), {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    };
};

export const removeFavoriteMusical = async (data: RemoveFavoriteDTO): Promise<void> => {
  await axios.delete(`${API_URL}/${data.accountId}/favorites/musicals/${data.targetId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const addFavoriteTheatre = async (data: AddFavoriteDTO): Promise<void> => {
  await axios.post(`${API_URL}/${data.accountId}/favorites/theatres/${data.targetId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const removeFavoriteTheatre = async (data: RemoveFavoriteDTO): Promise<void> => {
  await axios.delete(`${API_URL}/${data.accountId}/favorites/theatres/${data.targetId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const updateLastFavoritesView = async (accountId: number): Promise<void> => {
  await axios.put(`${API_URL}/${accountId}/favorites/last-view`, null, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
    }
  });
};

export const checkIsFavorite = async (data: {
  accountId: number;
  targetId: number;
  type: 'actor' | 'musical' | 'theatre';
}): Promise<boolean> => {
  const response = await axios.get<boolean>(
    `${API_URL}/${data.accountId}/favorites/check?targetId=${data.targetId}&type=${data.type}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    }
  );
  return response.data;
};

export const submitUpgradeRequest = async (accountId: number): Promise<void> => {
  await axios.post(`${API_URL}/${accountId}/upgrade-request`, null, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
    }
  });
};

export const getUpgradeRequests = async (): Promise<AccountDTO[]> => {
  const response = await axios.get<AccountDTO[]>(`${API_URL}/upgrade-requests`, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
    }
  });
  return response.data;
};

export const processUpgradeRequest = async (accountId: number, isApproved: boolean): Promise<void> => {
  await axios.post(`${API_URL}/${accountId}/process-upgrade`, { isApproved }, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
    }
  });
};