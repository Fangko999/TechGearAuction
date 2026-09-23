'use client';

import React, { useEffect, useState } from 'react';
import { useAdminUsers } from '@application/hooks/useAdminUsers';
import { UserDto } from '@domain/models/admin';

export default function UsersManagementPage() {
  const [searchTerm, setSearchTerm] = useState('');
  
  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalType, setModalType] = useState<'ban' | 'unban' | 'close' | 'restore'>('ban');
  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);
  const [reason, setReason] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { users, loading: isLoading, fetchUsers, banUser, unbanUser, closeAccount, restoreAccount } = useAdminUsers();

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const openModal = (user: UserDto, type: 'ban' | 'unban' | 'close' | 'restore') => {
    setSelectedUser(user);
    setModalType(type);
    setReason('');
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedUser(null);
    setReason('');
  };

  const handleSubmitAction = async () => {
    if (!selectedUser) return;
    
    // Ban and unban require a reason
    if ((modalType === 'ban' || modalType === 'unban') && !reason.trim()) {
      alert('Vui lÃƒÂ²ng nhÃ¡ÂºÂ­p lÃƒÂ½ do.');
      return;
    }

    setIsSubmitting(true);
    try {
      if (modalType === 'ban') {
        await banUser(selectedUser.id, reason);
      } else if (modalType === 'unban') {
        await unbanUser(selectedUser.id, reason);
      } else if (modalType === 'close') {
        await closeAccount(selectedUser.id);
      } else if (modalType === 'restore') {
        await restoreAccount(selectedUser.id);
      }
      
      closeModal();
      fetchUsers(searchTerm);
    } catch (err: any) {
      console.error(err);
      alert(`LÃ¡Â»â€”i: ${err?.response?.data?.Message || err.message}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  const getStatusBadge = (status: string | number) => {
    // 0: Active, 1: Banned, 2: Closed
    if (status === 'Active' || status === 0) return <span className="px-2 py-1 bg-green-900 text-green-300 rounded text-xs">HoÃ¡ÂºÂ¡t Ã„â€˜Ã¡Â»â„¢ng</span>;
    if (status === 'Banned' || status === 1) return <span className="px-2 py-1 bg-red-900 text-red-300 rounded text-xs">BÃ¡Â»â€¹ khÃƒÂ³a (Ban)</span>;
    if (status === 'Closed' || status === 2) return <span className="px-2 py-1 bg-gray-700 text-gray-400 rounded text-xs">Ã„ÂÃƒÂ£ Ã„â€˜ÃƒÂ³ng</span>;
    return <span className="px-2 py-1 bg-gray-700 text-gray-300 rounded text-xs">KhÃƒÂ´ng xÃƒÂ¡c Ã„â€˜Ã¡Â»â€¹nh</span>;
  };

  return (
    <div className="space-y-6 relative">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-white">QuÃ¡ÂºÂ£n lÃƒÂ½ NgÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng</h1>
          <p className="text-gray-400 text-sm mt-1">Danh sÃƒÂ¡ch ngÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng vÃƒÂ  trÃ¡ÂºÂ¡ng thÃƒÂ¡i hoÃ¡ÂºÂ¡t Ã„â€˜Ã¡Â»â„¢ng trÃƒÂªn hÃ¡Â»â€¡ thÃ¡Â»â€˜ng.</p>
        </div>
        <div className="flex space-x-3">
            <input 
              type="text" 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && fetchUsers(searchTerm)}
              placeholder="TÃƒÂ¬m kiÃ¡ÂºÂ¿m Email hoÃ¡ÂºÂ·c TÃƒÂªn..." 
              className="px-4 py-2 bg-gray-900 border border-gray-700 rounded-lg text-sm text-white focus:outline-none focus:border-blue-500 w-64"
            />
            <button 
              onClick={() => fetchUsers(searchTerm)}
              className="px-4 py-2 bg-gray-800 hover:bg-gray-700 border border-gray-700 text-white rounded-lg font-medium transition-colors"
            >
              TÃƒÂ¬m kiÃ¡ÂºÂ¿m
            </button>
        </div>
      </div>

      <div className="bg-gray-900 border border-gray-800 rounded-xl overflow-hidden">
        <table className="w-full text-left text-sm text-gray-300">
          <thead className="bg-gray-950 text-gray-400 text-xs uppercase border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 font-medium">NgÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng</th>
              <th className="px-6 py-4 font-medium">Vai trÃƒÂ²</th>
              <th className="px-6 py-4 font-medium">TrÃ¡ÂºÂ¡ng thÃƒÂ¡i</th>
              <th className="px-6 py-4 font-medium">SÃ¡Â»â€˜ lÃ¡ÂºÂ§n vi phÃ¡ÂºÂ¡m</th>
              <th className="px-6 py-4 font-medium text-right">Thao tÃƒÂ¡c</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800">
            {isLoading ? (
              <tr>
                <td colSpan={5} className="px-6 py-8 text-center text-gray-500">Ã„Âang tÃ¡ÂºÂ£i dÃ¡Â»Â¯ liÃ¡Â»â€¡u...</td>
              </tr>
            ) : users.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-8 text-center text-gray-500">KhÃƒÂ´ng tÃƒÂ¬m thÃ¡ÂºÂ¥y ngÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng nÃƒÂ o.</td>
              </tr>
            ) : (
              users.map((user) => (
                <tr key={user.id} className="hover:bg-gray-800/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-medium text-white">{user.displayName}</div>
                    <div className="text-gray-500 text-xs mt-1">{user.email}</div>
                  </td>
                  <td className="px-6 py-4">
                    {user.role === 'Admin' || user.role === '1' ? 'Admin' : 'NgÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng'}
                  </td>
                  <td className="px-6 py-4">
                    {getStatusBadge(user.status)}
                  </td>
                  <td className="px-6 py-4 text-orange-400">
                    {user.violationCount}
                  </td>
                  <td className="px-6 py-4 text-right space-x-3">
                    <button 
                      onClick={() => alert('ChÃ¡Â»Â©c nÃ„Æ’ng xem chi tiÃ¡ÂºÂ¿t hÃ¡Â»â€œ sÃ†Â¡ Ã„â€˜ang phÃƒÂ¡t triÃ¡Â»Æ’n.')}
                      className="text-blue-400 hover:text-blue-300 transition-colors"
                    >
                      Chi tiÃ¡ÂºÂ¿t
                    </button>
                    
                    {/* Ban / Unban actions */}
                    {(user.status === 'Active' || user.status === '0') && (
                      <button 
                        onClick={() => openModal(user, 'ban')}
                        className="text-red-400 hover:text-red-300 transition-colors"
                      >
                        Ban
                      </button>
                    )}
                    {(user.status === 'Banned' || user.status === '1') && (
                      <button 
                        onClick={() => openModal(user, 'unban')}
                        className="text-green-400 hover:text-green-300 transition-colors"
                      >
                        Unban
                      </button>
                    )}

                    {/* Close / Restore actions */}
                    {(user.status !== 'Closed' && user.status !== '2') ? (
                      <button 
                        onClick={() => openModal(user, 'close')}
                        className="text-gray-400 hover:text-gray-300 transition-colors"
                      >
                        Ã„ÂÃƒÂ³ng TK
                      </button>
                    ) : (
                      <button 
                        onClick={() => openModal(user, 'restore')}
                        className="text-yellow-400 hover:text-yellow-300 transition-colors"
                      >
                        KhÃƒÂ´i phÃ¡Â»Â¥c
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal Overlay */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="bg-gray-900 border border-gray-700 rounded-xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-xl font-bold text-white mb-2">
              {modalType === 'ban' && 'KhÃƒÂ³a tÃƒÂ i khoÃ¡ÂºÂ£n (Ban)'}
              {modalType === 'unban' && 'MÃ¡Â»Å¸ khÃƒÂ³a tÃƒÂ i khoÃ¡ÂºÂ£n (Unban)'}
              {modalType === 'close' && 'Ã„ÂÃƒÂ³ng tÃƒÂ i khoÃ¡ÂºÂ£n vÃ„Â©nh viÃ¡Â»â€¦n'}
              {modalType === 'restore' && 'KhÃƒÂ´i phÃ¡Â»Â¥c tÃƒÂ i khoÃ¡ÂºÂ£n'}
            </h3>
            
            <p className="text-gray-400 mb-6 text-sm">
              BÃ¡ÂºÂ¡n Ã„â€˜ang thÃ¡Â»Â±c hiÃ¡Â»â€¡n thao tÃƒÂ¡c trÃƒÂªn ngÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng <span className="font-bold text-white">{selectedUser?.email}</span>.
              {modalType === 'close' && ' HÃƒÂ nh Ã„â€˜Ã¡Â»â„¢ng nÃƒÂ y sÃ¡ÂºÂ½ Ã„â€˜ÃƒÂ¡nh dÃ¡ÂºÂ¥u tÃƒÂ i khoÃ¡ÂºÂ£n lÃƒÂ  Ã„ÂÃƒÂ£ Ã„â€˜ÃƒÂ³ng (Closed).'}
              {modalType === 'restore' && ' HÃƒÂ nh Ã„â€˜Ã¡Â»â„¢ng nÃƒÂ y sÃ¡ÂºÂ½ khÃƒÂ´i phÃ¡Â»Â¥c tÃƒÂ i khoÃ¡ÂºÂ£n vÃ¡Â»Â trÃ¡ÂºÂ¡ng thÃƒÂ¡i Active.'}
            </p>

            {(modalType === 'ban' || modalType === 'unban') && (
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  LÃƒÂ½ do <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  className="w-full bg-gray-800 border border-gray-700 rounded-lg p-3 text-white placeholder-gray-500 focus:outline-none focus:border-blue-500"
                  rows={3}
                  placeholder={modalType === 'ban' ? 'NhÃ¡ÂºÂ­p lÃƒÂ½ do khÃƒÂ³a tÃƒÂ i khoÃ¡ÂºÂ£n...' : 'NhÃ¡ÂºÂ­p lÃƒÂ½ do mÃ¡Â»Å¸ khÃƒÂ³a...'}
                />
              </div>
            )}

            <div className="flex justify-end space-x-3">
              <button
                onClick={closeModal}
                disabled={isSubmitting}
                className="px-4 py-2 text-gray-400 hover:bg-gray-800 rounded-lg transition-colors"
              >
                HÃ¡Â»Â§y
              </button>
              <button
                onClick={handleSubmitAction}
                disabled={isSubmitting}
                className={`px-4 py-2 text-white rounded-lg transition-colors font-medium ${
                  modalType === 'ban' || modalType === 'close' ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'
                } disabled:opacity-50`}
              >
                {isSubmitting ? 'Ã„Âang xÃ¡Â»Â­ lÃƒÂ½...' : 'XÃƒÂ¡c nhÃ¡ÂºÂ­n'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
