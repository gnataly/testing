import axios from 'axios';
import { RoleDTO } from '../types';

const API_URL = 'https://localhost:5001/api/roles';

export const getAllRoles = async (): Promise<RoleDTO[]> => {
  const response = await axios.get<RoleDTO[]>(API_URL);
  return response.data;
};

export const getRoleById = async (id: number): Promise<RoleDTO> => {
  const response = await axios.get<RoleDTO>(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const createRole = async (roleData: Omit<RoleDTO, 'id'>): Promise<RoleDTO> => {
  const response = await axios.post<RoleDTO>(API_URL, roleData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const updateRole = async (id: number, roleData: Partial<RoleDTO>): Promise<RoleDTO> => {
  const response = await axios.put<RoleDTO>(`${API_URL}/${id}`, roleData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};

export const deleteRole = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
};

export const getRolesByMusical = async (musicalId: number): Promise<RoleDTO[]> => {
  const response = await axios.get<RoleDTO[]>(`${API_URL}/musical/${musicalId}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
  return response.data;
};