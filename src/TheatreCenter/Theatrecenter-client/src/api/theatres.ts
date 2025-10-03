import axios from 'axios';
import { TheatreDTO } from '../types';

const API_URL = 'https://localhost:5001/api/theatres';

export const getAllTheatres = async (): Promise<TheatreDTO[]> => {
  const response = await axios.get<TheatreDTO[]>(API_URL, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const getTheatreById = async (id: number): Promise<TheatreDTO> => {
  const response = await axios.get<TheatreDTO>(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const createTheatre = async (theatreData: Omit<TheatreDTO, 'id'>): Promise<TheatreDTO> => {
  const response = await axios.post<TheatreDTO>(API_URL, theatreData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateTheatre = async (id: number, theatreData: Partial<TheatreDTO>): Promise<TheatreDTO> => {
  const response = await axios.put<TheatreDTO>(`${API_URL}/${id}`, theatreData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteTheatre = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};