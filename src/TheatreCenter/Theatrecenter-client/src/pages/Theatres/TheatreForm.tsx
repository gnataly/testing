import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getTheatreById, createTheatre, updateTheatre } from '../../api/theatres';

const TheatreForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: '',
    addInfo: ''
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchTheatre = async () => {
        try {
          const theatre = await getTheatreById(Number(id));
          setFormData({
            name: theatre.name,
            addInfo: theatre.addInfo || ''
          });
        } catch (error) {
          console.error('Ошибка при загрузке театра:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchTheatre();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateTheatre(Number(id), formData);
      } else {
        await createTheatre(formData);
      }
      navigate('/theatres');
    } catch (error) {
      console.error('Ошибка при сохранении театра:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать театр' : 'Добавить новый театр'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div className="form-control">
          <label className="label">
            <span className="label-text">Название</span>
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
            <span className="label-text">Дополнительная информация</span>
          </label>
          <textarea
            name="addInfo"
            value={formData.addInfo}
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
            onClick={() => navigate('/theatres')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default TheatreForm;