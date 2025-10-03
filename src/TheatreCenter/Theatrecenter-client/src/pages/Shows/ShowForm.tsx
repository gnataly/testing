import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getShowById, createShow, updateShow } from '../../api/shows';

const ShowForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    date: '',
    musicalId: 0
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchShow = async () => {
        try {
          const show = await getShowById(Number(id));
          setFormData({
            date: show.date.split('.')[0], // Remove milliseconds
            musicalId: show.musicalId
          });
        } catch (error) {
          console.error('Ошибка при загрузке спектакля:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchShow();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
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
        await updateShow(Number(id), formData);
      } else {
        await createShow(formData);
      }
      navigate('/shows');
    } catch (error) {
      console.error('Ошибка при сохранении спектакля:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать спектакль' : 'Добавить новый спектакль'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div className="form-control">
          <label className="label">
            <span className="label-text">Дата и время</span>
          </label>
          <input
            type="datetime-local"
            name="date"
            value={formData.date}
            onChange={handleChange}
            className="input input-bordered w-full"
            required
          />
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
          />
        </div>

        <div className="flex space-x-2">
          <button type="submit" className="btn btn-primary">
            {id ? 'Обновить' : 'Создать'}
          </button>
          <button 
            type="button" 
            onClick={() => navigate('/shows')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default ShowForm;