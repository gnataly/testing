import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getActorById, createActor, updateActor } from '../../api/actors';
import { VoiceType, Gender, voiceTypeOptions, genderOptions } from '../../types';

interface ActorFormData {
  name: string;
  voiceType: VoiceType;
  gender: Gender;
  birthDate: string;
  height: number;
  weight: number;
  addInfo?: string;
}

const ActorForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState<ActorFormData>({
    name: '',
    voiceType: VoiceType.Tenor,
    gender: Gender.Male,
    birthDate: '',
    height: 175,
    weight: 60,
    addInfo: ''
  });
  const [loading, setLoading] = useState(!!id);

  useEffect(() => {
    if (id) {
      const fetchActor = async () => {
        try {
          const actor = await getActorById(Number(id));
          setFormData({
            name: actor.name,
            voiceType: actor.voiceType as VoiceType,
            gender: actor.gender as Gender,
            birthDate: actor.birthDate.split('T')[0],
            height: actor.height,
            weight: actor.weight,
            addInfo: actor.addInfo || ''
          });
        } catch (error) {
          console.error('Ошибка при загрузке актера:', error);
        } finally {
          setLoading(false);
        }
      };
      fetchActor();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'height' || name === 'weight' ? Number(value) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const actorData = {
        ...formData,
        birthDate: new Date(formData.birthDate).toISOString(),
      };
  
      if (id) {
        await updateActor(Number(id), actorData);
      } else {
        await createActor(actorData);
      }
      navigate('/actors');
    } catch (error) {
      console.error('Ошибка при сохранении актера:', error);
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">
        {id ? 'Редактировать актера' : 'Добавить нового актера'}
      </h1>
      
      <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
        {/* Поле имени */}
        {/* <div className="form-control">
          <label className="label">
            <span className="label-text">Имя</span>
          </label>
          <input
            type="text"
            name="name"
            value={formData.name}
            onChange={handleChange}
            className="input input-bordered w-full"
            required
          />
        </div> */}
        <div className="form-control">
          <label className="input-label">Имя</label>
          <input
            type="text"
            name="name"
            value={formData.name}
            onChange={handleChange}
            className="input-field"
            required
          />
        </div>

        {/* Выбор типа голоса */}
        {/* <div className="form-control">
          <label className="label">
            <span className="label-text">Тип голоса</span>
          </label>
          <select
            name="voiceType"
            value={formData.voiceType}
            onChange={handleChange}
            className="select select-bordered w-full"
            required
          >
            {voiceTypeOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div> */}
        <div className="form-control">
          <label className="input-label">Тип голоса</label>
          <select
            name="voiceType"
            value={formData.voiceType}
            onChange={handleChange}
            className="select-field"
            required
          >
            {voiceTypeOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        {/* Выбор пола */}
        <div className="form-control">
          <label className="label">
            <span className="label-text">Пол</span>
          </label>
          <select
            name="gender"
            value={formData.gender}
            onChange={handleChange}
            className="select select-bordered w-full"
            required
          >
            {genderOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        {/* Дата рождения */}
        <div className="form-control">
          <label className="label">
            <span className="label-text">Дата рождения</span>
          </label>
          <input
            type="date"
            name="birthDate"
            value={formData.birthDate}
            onChange={handleChange}
            className="input input-bordered w-full"
            required
          />
        </div>

        {/* Рост и вес */}
        <div className="grid grid-cols-2 gap-4">
          <div className="form-control">
            <label className="label">
              <span className="label-text">Рост (см)</span>
            </label>
            <input
              type="number"
              name="height"
              value={formData.height}
              onChange={handleChange}
              className="input input-bordered w-full"
              min="100"
              max="250"
              required
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">Вес (кг)</span>
            </label>
            <input
              type="number"
              name="weight"
              value={formData.weight}
              onChange={handleChange}
              className="input input-bordered w-full"
              min="30"
              max="300"
              required
            />
          </div>
        </div>

        {/* Дополнительная информация */}
        <div className="form-control">
          <label className="label">
            <span className="label-text">Дополнительная информация</span>
          </label>
          <textarea
            name="addInfo"
            value={formData.addInfo || ''}
            onChange={handleChange}
            className="textarea textarea-bordered w-full"
            rows={3}
          />
        </div>

        {/* <div className="form-control">
          <label className="input-label">Дополнительная информация</label>
          <textarea
            name="addInfo"
            value={formData.addInfo || ''}
            onChange={handleChange}
            className="textarea-field"
            rows={3}
          />
        </div> */}

        {/* Кнопки */}
        <div className="flex space-x-2">
          <button type="submit" className="btn btn-primary">
            {id ? 'Обновить' : 'Создать'}
          </button>
          <button 
            type="button" 
            onClick={() => navigate('/actors')} 
            className="btn btn-outline"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
};

export default ActorForm;