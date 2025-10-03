import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getCastMemberById, createCastMember, updateCastMember } from '../../api/castMembers';

const CastMemberForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    showId: 0,
    roleId: 0,
    actorId: 0,
    comment: ''
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchCastMember = async () => {
        try {
          const cm = await getCastMemberById(Number(id));
          setFormData({
            showId: cm.showId,
            roleId: cm.roleId,
            actorId: cm.actorId,
            comment: cm.comment || ''
          });
        } catch (error) {
          console.error('Ошибка при загрузке участника труппы:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchCastMember();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'showId' || name === 'roleId' || name === 'actorId' 
        ? Number(value) 
        : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateCastMember(Number(id), formData);
      } else {
        await createCastMember(formData);
      }
      navigate('/cast-members');
    } catch (error) {
      console.error('Ошибка при сохранении участника труппы:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать участника труппы' : 'Добавить нового участника труппы'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div className="grid grid-cols-2 gap-4">
          <div className="form-control">
            <label className="label">
              <span className="label-text">ID спектакля</span>
            </label>
            <input
              type="number"
              name="showId"
              value={formData.showId}
              onChange={handleChange}
              className="input input-bordered w-full"
              min="1"
              required
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">ID роли</span>
            </label>
            <input
              type="number"
              name="roleId"
              value={formData.roleId}
              onChange={handleChange}
              className="input input-bordered w-full"
              min="1"
              required
            />
          </div>
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">ID актера</span>
          </label>
          <input
            type="number"
            name="actorId"
            value={formData.actorId}
            onChange={handleChange}
            className="input input-bordered w-full"
            min="1"
            required
          />
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">Комментарий</span>
          </label>
          <textarea
            name="comment"
            value={formData.comment}
            onChange={handleChange}
            className="textarea textarea-bordered w-full"
            rows={3}
          />
        </div>

        <div className="flex space-x-2">
          <button type="submit" className="btn btn-primary">
            {id ? 'Обновить' : 'Создать'}
          </button>
          <button 
            type="button" 
            onClick={() => navigate('/cast-members')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default CastMemberForm;