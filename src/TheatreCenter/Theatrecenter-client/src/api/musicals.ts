import axios from 'axios';
import { MusicalDTO } from '../types';

const API_URL = 'https://localhost:5001/api/musicals';

export const getAllMusicals = async (): Promise<MusicalDTO[]> => {
  const response = await axios.get<MusicalDTO[]>(API_URL, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const getMusicalById = async (id: number): Promise<MusicalDTO> => {
  const response = await axios.get<MusicalDTO>(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const createMusical = async (musicalData: Omit<MusicalDTO, 'id'>): Promise<MusicalDTO> => {
  const response = await axios.post<MusicalDTO>(API_URL, musicalData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateMusical = async (id: number, musicalData: Partial<MusicalDTO>): Promise<MusicalDTO> => {
  const response = await axios.put<MusicalDTO>(`${API_URL}/${id}`, musicalData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteMusical = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const getMusicalsByTheatre = async (theatreId: number): Promise<MusicalDTO[]> => {
  const response = await axios.get<MusicalDTO[]>(`${API_URL}/theatre/${theatreId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const getMusicalsByAgeRestriction = async (ageRestriction: string): Promise<MusicalDTO[]> => {
  const response = await axios.get<MusicalDTO[]>(`${API_URL}/age-restriction/${ageRestriction}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};