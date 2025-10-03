import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { getRoleById, createRole, updateRole } from '../../api/roles';
import { RoleType, roleTypeOptions } from '../../types';

const RoleForm: React.FC = () => {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: '',
    roleType: RoleType.Main,
    musicalId: Number(searchParams.get('musicalId')) || 0
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchRole = async () => {
        try {
          const role = await getRoleById(Number(id));
          setFormData({
            name: role.name,
            roleType: role.roleType,
            musicalId: role.musicalId
          });
        } catch (error) {
          console.error('Ошибка при загрузке роли:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchRole();
    } else {
      setLoading(false);
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'musicalId' ? Number(value) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateRole(Number(id), formData);
      } else {
        await createRole(formData);
      }
      navigate('/roles');
    } catch (error) {
      console.error('Ошибка при сохранении роли:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать роль' : 'Добавить новую роль'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div className="form-control">
          <label className="label">
            <span className="label-text">Название роли</span>
          </label>
          <input
            type="text"
            name="name"
            value={formData.name}
            onChange={handleChange}
            className="input input-bordered w-full"
            required
          />
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">Тип роли</span>
          </label>
          <select
            name="roleType"
            value={formData.roleType}
            onChange={handleChange}
            className="select select-bordered w-full"
            required
          >
            {roleTypeOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">ID мюзикла</span>
          </label>
          <input
            type="number"
            name="musicalId"
            value={formData.musicalId}
            onChange={handleChange}
            className="input input-bordered w-full"
            min="1"
            required
            disabled={!!searchParams.get('musicalId')}
          />
        </div>

        <div className="flex space-x-2">
          <button type="submit" className="btn btn-primary">
            {id ? 'Обновить' : 'Создать'}
          </button>
          <button 
            type="button" 
            onClick={() => navigate('/roles')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default RoleForm;