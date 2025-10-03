import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { getUpgradeRequests, processUpgradeRequest } from '../../api/accounts';
import { AccountDTO } from '../../types';

const UpgradeRequestsPage: React.FC = () => {
  const { user } = useAuth();
  const [requests, setRequests] = useState<AccountDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<number | null>(null);

  useEffect(() => {
    const fetchRequests = async () => {
      try {
        const data = await getUpgradeRequests();
        setRequests(data);
      } catch (error) {
        console.error('Failed to fetch upgrade requests:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchRequests();
  }, []);

  const handleProcessRequest = async (accountId: number, isApproved: boolean) => {
    try {
      setProcessing(accountId);
      await processUpgradeRequest(accountId, isApproved);
      setRequests(prev => prev.filter(acc => acc.id !== accountId));
    } catch (error) {
      console.error('Failed to process upgrade request:', error);
    } finally {
      setProcessing(null);
    }
  };

  if (!user || user.accessLevel !== 'Admin') {
    return <div className="container mx-auto px-4 py-8">Доступ запрещен</div>;
  }

  if (loading) {
    return <div className="container mx-auto px-4 py-8">Загрузка...</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Заявки на повышение статуса</h1>
      
      {requests.length === 0 ? (
        <p>Нет активных заявок</p>
      ) : (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Имя пользователя</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Действия</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {requests.map(account => (
                <tr key={account.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{account.id}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{account.username}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex space-x-2">
                      <button
                        onClick={() => handleProcessRequest(account.id, true)}
                        disabled={processing === account.id}
                        className="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600 disabled:opacity-50"
                      >
                        {processing === account.id ? 'Обработка...' : 'Одобрить'}
                      </button>
                      <button
                        onClick={() => handleProcessRequest(account.id, false)}
                        disabled={processing === account.id}
                        className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600 disabled:opacity-50"
                      >
                        {processing === account.id ? 'Обработка...' : 'Отклонить'}
                      </button>
                    </div>
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

export default UpgradeRequestsPage;