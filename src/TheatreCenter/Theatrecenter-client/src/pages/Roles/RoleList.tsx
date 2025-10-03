import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllRoles, deleteRole } from '../../api/roles';
import { RoleType, roleTypeTranslations } from '../../types';

const RoleList: React.FC = () => {
  const [roles, setRoles] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const data = await getAllRoles();
        setRoles(data);
      } catch (err) {
        setError('Ошибка при загрузке ролей');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchRoles();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteRole(id);
      setRoles(roles.filter(role => role.id !== id));
    } catch (err) {
      setError('Ошибка при удалении роли');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;
  if (roles.length === 0) return <div className="p-4">Роли не найдены</div>;

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Роли</h1>
        <Link 
          to="/roles/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить роль
        </Link>
      </div>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left text-sm">ID</th>
              <th className="py-2 px-4 border text-left text-sm">Название</th>
              <th className="py-2 px-4 border text-left text-sm">Тип роли</th>
              <th className="py-2 px-4 border text-left text-sm">ID мюзикла</th>
              <th className="py-2 px-4 border text-left text-sm">Действия</th>
            </tr>
          </thead>
          <tbody>
            {roles.map(role => (
              <tr key={role.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border text-sm">{role.id}</td>
                <td className="py-2 px-4 border text-sm">
                  <Link to={`/roles/${role.id}`} className="text-blue-500 hover:underline">
                    {role.name}
                  </Link>
                </td>
                <td className="py-2 px-4 border text-sm">{roleTypeTranslations[role.roleType as RoleType]}</td>
                {/* <td className="py-2 px-4 border text-sm">{role.musicalId}</td> */}
                <td className="py-2 px-4 border text-sm">
                  <Link to={`/musicals/${role.musicalId}`} className="text-blue-500 hover:underline">
                    {role.musicalId}
                  </Link>
                </td>
                <td className="py-2 px-4 border text-sm">
                  <div className="flex space-x-2">
                    <Link
                      to={`/roles/${role.id}/edit`}
                      className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                    >
                      Редактировать
                    </Link>
                    <button
                      onClick={() => handleDelete(role.id)}
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

export default RoleList;