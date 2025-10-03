import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getRoleById } from '../../api/roles';
import { RoleType, roleTypeTranslations } from '../../types';

const RoleDetails: React.FC = () => {
  const { id } = useParams();
  const [role, setRole] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRole = async () => {
      try {
        const data = await getRoleById(Number(id));
        setRole(data);
      } catch (error) {
        console.error('Error fetching role:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchRole();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!role) return <div>Role not found</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">{role.name}</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <p><strong>Тип роли:</strong> {roleTypeTranslations[role.roleType as RoleType]}</p>
        </div>
        <div>
          <p><strong>ID мюзикла:</strong> {role.musicalId}</p>
        </div>
      </div>
    </div>
  );
};

export default RoleDetails;