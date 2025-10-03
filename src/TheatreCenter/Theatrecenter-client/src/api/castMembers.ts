import axios from 'axios';
import { CastMemberDTO } from '../types';

// const API_URL = 'https://localhost:5001/api/cast-members';
const API_URL = 'https://localhost:5001/api/CastMembers';

export const getAllCastMembers = async (): Promise<CastMemberDTO[]> => {
  const response = await axios.get<CastMemberDTO[]>(API_URL);
  return response.data;
};

export const getCastMemberById = async (id: number): Promise<CastMemberDTO> => {
  const response = await axios.get<CastMemberDTO>(`${API_URL}/${id}`);
  return response.data;
};

export const createCastMember = async (castMemberData: Omit<CastMemberDTO, 'id'>): Promise<CastMemberDTO> => {
  const response = await axios.post<CastMemberDTO>(API_URL, castMemberData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateCastMember = async (id: number, castMemberData: Partial<CastMemberDTO>): Promise<CastMemberDTO> => {
  const response = await axios.put<CastMemberDTO>(`${API_URL}/${id}`, castMemberData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteCastMember = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const getCastMembersByShow = async (showId: number): Promise<CastMemberDTO[]> => {
  const response = await axios.get<CastMemberDTO[]>(`${API_URL}/show/${showId}`);
  return response.data;
};