import axios from 'axios';
import { ShowDTO } from '../types';

const API_URL = 'https://localhost:5001/api/shows';

export const getAllShows = async (): Promise<ShowDTO[]> => {
  const response = await axios.get<ShowDTO[]>(API_URL);
  return response.data;
};

export const getShowById = async (id: number): Promise<ShowDTO> => {
  const response = await axios.get<ShowDTO>(`${API_URL}/${id}`);
  return response.data;
};

export const createShow = async (showData: Omit<ShowDTO, 'id'>): Promise<ShowDTO> => {
  const response = await axios.post<ShowDTO>(API_URL, showData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateShow = async (id: number, showData: Partial<ShowDTO>): Promise<ShowDTO> => {
  const response = await axios.put<ShowDTO>(`${API_URL}/${id}`, showData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteShow = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const getShowsByMusical = async (musicalId: number): Promise<ShowDTO[]> => {
  const response = await axios.get<ShowDTO[]>(`${API_URL}/musical/${musicalId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};