import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getRolesByMusical } from '../../api/roles';
import { RoleType, roleTypeTranslations } from '../../types';

const MusicalRoles: React.FC = () => {
  const { id } = useParams();
  const [roles, setRoles] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const data = await getRolesByMusical(Number(id));
        setRoles(data);
      } catch (error) {
        console.error('Error fetching roles:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchRoles();
  }, [id]);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Роли мюзикла</h1>
        <Link
          to={`/roles/create?musicalId=${id}`}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Добавить роль
        </Link>
      </div>
      
      {roles.length === 0 ? (
        <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 text-center">
          <p className="mb-4">Роли не найдены</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border">
            <thead>
              <tr className="bg-gray-100">
                <th className="py-2 px-4 border text-left">ID</th>
                <th className="py-2 px-4 border text-left">Название</th>
                <th className="py-2 px-4 border text-left">Тип роли</th>
                <th className="py-2 px-4 border text-left">Действия</th>
              </tr>
            </thead>
            <tbody>
              {roles.map(role => (
                <tr key={role.id} className="hover:bg-gray-50">
                  <td className="py-2 px-4 border">{role.id}</td>
                  <td className="py-2 px-4 border">
                    <Link 
                      to={`/roles/${role.id}`}
                      className="text-blue-500 hover:underline"
                    >
                      {role.name}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border">{roleTypeTranslations[role.roleType as RoleType]}</td>
                  <td className="py-2 px-4 border">
                    <Link 
                      to={`/roles/${role.id}/edit`}
                      className="text-blue-500 hover:underline"
                    >
                      Редактировать
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default MusicalRoles;