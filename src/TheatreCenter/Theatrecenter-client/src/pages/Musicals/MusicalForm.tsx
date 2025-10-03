import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getMusicalById, createMusical, updateMusical } from '../../api/musicals';
import { AgeRestriction, ageRestrictionOptions } from '../../types';

const MusicalForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    duration: '',
    ageRestriction: AgeRestriction.AllAges,
    theatreId: 0
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchMusical = async () => {
        try {
          const musical = await getMusicalById(Number(id));
          setFormData({
            title: musical.title,
            description: musical.description,
            duration: musical.duration,
            ageRestriction: musical.ageRestriction,
            theatreId: musical.theatreId
          });
        } catch (error) {
          console.error('Ошибка при загрузке мюзикла:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchMusical();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'theatreId' ? Number(value) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (id) {
        await updateMusical(Number(id), formData);
      } else {
        await createMusical(formData);
      }
      navigate('/musicals');
    } catch (error) {
      console.error('Ошибка при сохранении мюзикла:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать мюзикл' : 'Добавить новый мюзикл'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        <div className="form-control">
          <label className="label">
            <span className="label-text">Название</span>
          </label>
          <input
            type="text"
            name="title"
            value={formData.title}
            onChange={handleChange}
            className="input input-bordered w-full"
            required
          />
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">Описание</span>
          </label>
          <textarea
            name="description"
            value={formData.description}
            onChange={handleChange}
            className="textarea textarea-bordered w-full"
            rows={3}
            required
          />
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">Длительность (например, PT2H30M)</span>
          </label>
          <input
            type="text"
            name="duration"
            value={formData.duration}
            onChange={handleChange}
            className="input input-bordered w-full"
            placeholder="ISO 8601 формат (PT2H30M)"
            required
          />
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">Возрастное ограничение</span>
          </label>
          <select
            name="ageRestriction"
            value={formData.ageRestriction}
            onChange={handleChange}
            className="select select-bordered w-full"
            required
          >
            {ageRestrictionOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div className="form-control">
          <label className="label">
            <span className="label-text">ID театра</span>
          </label>
          <input
            type="number"
            name="theatreId"
            value={formData.theatreId}
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
            onClick={() => navigate('/musicals')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default MusicalForm;