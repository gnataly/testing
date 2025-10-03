import axios from 'axios';
import { ActorDTO } from '../types';

const API_URL = 'https://localhost:5001/api/actors';

export const getAllActors = async (): Promise<ActorDTO[]> => {
  const response = await axios.get<ActorDTO[]>(API_URL, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const getActorById = async (id: number): Promise<ActorDTO> => {
  const response = await axios.get<ActorDTO>(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const createActor = async (actorData: Omit<ActorDTO, 'id'>): Promise<ActorDTO> => {
  const response = await axios.post<ActorDTO>(API_URL, actorData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateActor = async (id: number, actorData: Partial<ActorDTO>): Promise<ActorDTO> => {
  const response = await axios.put<ActorDTO>(`${API_URL}/${id}`, actorData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteActor = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};