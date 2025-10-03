import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllCastMembers, deleteCastMember } from '../../api/castMembers';

const CastMemberList: React.FC = () => {
  const [castMembers, setCastMembers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCastMembers = async () => {
      try {
        const data = await getAllCastMembers();
        setCastMembers(data);
      } catch (err) {
        setError('Ошибка при загрузке участников труппы');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchCastMembers();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteCastMember(id);
      setCastMembers(castMembers.filter(cm => cm.id !== id));
    } catch (err) {
      setError('Ошибка при удалении участника труппы');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;
  if (castMembers.length === 0) return <div className="p-4">Участники труппы не найдены</div>;

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Участники труппы</h1>
        <Link 
          to="/cast-members/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить участника
        </Link>
      </div>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left text-sm">ID</th>
              <th className="py-2 px-4 border text-left text-sm">ID спектакля</th>
              <th className="py-2 px-4 border text-left text-sm">ID роли</th>
              <th className="py-2 px-4 border text-left text-sm">ID актера</th>
              <th className="py-2 px-4 border text-left text-sm">Комментарий</th>
              <th className="py-2 px-4 border text-left text-sm">Действия</th>
            </tr>
          </thead>
          <tbody>
            {castMembers.map(cm => (
              <tr key={cm.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border text-sm">{cm.id}</td>
                <td className="py-2 px-4 border text-sm">
                  <Link to={`/cast-members/${cm.id}`} className="text-blue-500 hover:underline">
                    {cm.showId}
                  </Link>
                </td>
                <td className="py-2 px-4 border text-sm">{cm.roleId}</td>
                <td className="py-2 px-4 border text-sm">{cm.actorId}</td>
                <td className="py-2 px-4 border text-sm">{cm.comment || '-'}</td>
                <td className="py-2 px-4 border text-sm">
                  <div className="flex space-x-2">
                    <Link
                      to={`/cast-members/${cm.id}/edit`}
                      className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                    >
                      Редактировать
                    </Link>
                    <button
                      onClick={() => handleDelete(cm.id)}
                      className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600 transition-colors"
                    >
                      Удалить
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default CastMemberList;