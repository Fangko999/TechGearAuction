'use client';

import React, { useState, useRef, useCallback } from 'react';
import { useCreateAuction } from '@application/hooks/useCreateAuction';

export default function CreateAuctionPage() {
  const { handleCreateAuction, isLoading, error, success } = useCreateAuction();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [startPrice, setStartPrice] = useState<number | ''>('');
  const [bidIncrement, setBidIncrement] = useState<number | ''>('');
  const [buyNowPrice, setBuyNowPrice] = useState<number | ''>('');
  
  // Máº·c Ä‘á»‹nh thá»i gian: Báº¯t Ä‘áº§u tá»« hiá»‡n táº¡i + 1 giá», Káº¿t thÃºc + 24 giá»
  const defaultStart = new Date(Date.now() + 3600000).toISOString().slice(0, 16);
  const defaultEnd = new Date(Date.now() + 86400000 + 3600000).toISOString().slice(0, 16);
  
  const [startTime, setStartTime] = useState(defaultStart);
  const [endTime, setEndTime] = useState(defaultEnd);
  
  // Category hardcode máº«u, trong thá»±c táº¿ sáº½ fetch tá»« API GetCategories
  const [categoryId, setCategoryId] = useState('d3b07384-d9a7-4b7b-b35f-155e99859f51'); 

  const [files, setFiles] = useState<File[]>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const onDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const onDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const droppedFiles = Array.from(e.dataTransfer.files).filter(f => f.type.startsWith('image/'));
      setFiles(prev => [...prev, ...droppedFiles].slice(0, 10)); // Max 10 hÃ¬nh theo logic backend
    }
  }, []);

  const onFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const selectedFiles = Array.from(e.target.files).filter(f => f.type.startsWith('image/'));
      setFiles(prev => [...prev, ...selectedFiles].slice(0, 10));
    }
  };

  const removeFile = (index: number) => {
    setFiles(prev => prev.filter((_, i) => i !== index));
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    await handleCreateAuction({
      title,
      description,
      startPrice: Number(startPrice),
      bidIncrement: Number(bidIncrement),
      buyNowPrice: buyNowPrice ? Number(buyNowPrice) : undefined,
      startTime: new Date(startTime).toISOString(),
      endTime: new Date(endTime).toISOString(),
      categoryId
    }, files);
  };

  if (success) {
    return (
      <div className="max-w-3xl p-8 mx-auto mt-10 bg-white rounded-xl shadow-sm text-center">
        <div className="flex justify-center mb-4">
          <div className="flex items-center justify-center w-16 h-16 bg-green-100 rounded-full">
            <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
          </div>
        </div>
        <h2 className="text-2xl font-bold text-gray-900">Táº¡o phiÃªn Ä‘áº¥u giÃ¡ thÃ nh cÃ´ng!</h2>
        <p className="mt-2 text-gray-600">Sáº£n pháº©m cá»§a báº¡n Ä‘Ã£ Ä‘Æ°á»£c chuyá»ƒn vÃ o tráº¡ng thÃ¡i Draft. HÃ£y kiá»ƒm tra láº¡i thÃ´ng tin trÆ°á»›c khi Active.</p>
        <button onClick={() => window.location.reload()} className="px-6 py-2 mt-6 font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">Táº¡o thÃªm sáº£n pháº©m</button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">ÄÄƒng bÃ¡n sáº£n pháº©m má»›i</h1>
      
      {error && (
        <div className="p-4 mb-6 text-sm text-red-700 bg-red-50 border-l-4 border-red-500 rounded-r-md">
          <p className="font-medium">Lá»—i xáº£y ra:</p>
          <p>{error}</p>
        </div>
      )}

      <form onSubmit={onSubmit} className="space-y-8 bg-white p-6 rounded-xl shadow-sm">
        {/* Basic Info */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700">TÃªn sáº£n pháº©m *</label>
            <input type="text" required value={title} onChange={(e) => setTitle(e.target.value)} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="VÃ­ dá»¥: ÄÃ¨n pin EDC Olight..." />
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700">MÃ´ táº£ chi tiáº¿t</label>
            <textarea rows={4} value={description} onChange={(e) => setDescription(e.target.value)} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Cung cáº¥p chi tiáº¿t tÃ¬nh tráº¡ng, xuáº¥t xá»©..."></textarea>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">GiÃ¡ khá»Ÿi Ä‘iá»ƒm (VNÄ) *</label>
            <input type="number" required min="1000" value={startPrice} onChange={(e) => setStartPrice(e.target.value ? Number(e.target.value) : '')} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">BÆ°á»›c giÃ¡ tá»‘i thiá»ƒu (VNÄ) *</label>
            <input type="number" required min="1000" value={bidIncrement} onChange={(e) => setBidIncrement(e.target.value ? Number(e.target.value) : '')} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">GiÃ¡ Mua ngay (KhÃ´ng báº¯t buá»™c)</label>
            <input type="number" min="1000" value={buyNowPrice} onChange={(e) => setBuyNowPrice(e.target.value ? Number(e.target.value) : '')} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Bá» trá»‘ng náº¿u khÃ´ng cho mua Ä‘á»©t" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Danh má»¥c</label>
            <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500">
              <option value="d3b07384-d9a7-4b7b-b35f-155e99859f51">ÄÃ¨n Pin EDC</option>
              <option value="d3b07384-d9a7-4b7b-b35f-155e99859f52">TÃºi Äeo ChÃ©o</option>
              <option value="d3b07384-d9a7-4b7b-b35f-155e99859f53">Dao Äa NÄƒng</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Thá»i gian báº¯t Ä‘áº§u *</label>
            <input type="datetime-local" required value={startTime} onChange={(e) => setStartTime(e.target.value)} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Thá»i gian káº¿t thÃºc *</label>
            <input type="datetime-local" required value={endTime} onChange={(e) => setEndTime(e.target.value)} disabled={isLoading}
              className="w-full px-4 py-2 mt-1 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>
        </div>

        {/* Image Upload Drag & Drop */}
        <div className="pt-4 border-t border-gray-200">
          <label className="block text-sm font-medium text-gray-700 mb-2">HÃ¬nh áº£nh sáº£n pháº©m (Tá»‘i Ä‘a 10 áº£nh)</label>
          
          <div 
            onDragOver={onDragOver} onDragLeave={onDragLeave} onDrop={onDrop}
            className={`flex flex-col items-center justify-center w-full h-32 px-4 py-6 border-2 border-dashed rounded-lg transition-colors cursor-pointer ${
              isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 bg-gray-50 hover:bg-gray-100'
            }`}
            onClick={() => fileInputRef.current?.click()}
          >
            <svg className="w-10 h-10 text-gray-400 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
            <p className="text-sm text-gray-500"><span className="font-medium text-blue-600">Báº¥m Ä‘á»ƒ táº£i lÃªn</span> hoáº·c kÃ©o tháº£ áº£nh vÃ o Ä‘Ã¢y</p>
            <input type="file" ref={fileInputRef} className="hidden" multiple accept="image/*" onChange={onFileSelect} disabled={isLoading} />
          </div>

          {/* Image Previews */}
          {files.length > 0 && (
            <div className="grid grid-cols-2 gap-4 mt-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
              {files.map((file, index) => (
                <div key={index} className="relative group">
                  <img src={URL.createObjectURL(file)} alt="preview" className="object-cover w-full h-24 rounded-lg shadow-sm" />
                  <button type="button" onClick={() => removeFile(index)} disabled={isLoading}
                    className="absolute top-1 right-1 p-1 text-white bg-red-500 rounded-full opacity-0 group-hover:opacity-100 transition-opacity">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                  </button>
                  {index === 0 && <span className="absolute bottom-1 left-1 px-2 py-0.5 text-xs text-white bg-blue-600 rounded-md">áº¢nh chÃ­nh</span>}
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="flex justify-end pt-4 border-t border-gray-200">
          <button type="submit" disabled={isLoading}
            className="flex items-center px-6 py-2 text-sm font-medium text-white transition-colors bg-blue-600 rounded-md shadow-sm hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
            {isLoading && (
              <svg className="w-5 h-5 mr-2 text-white animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            )}
            {isLoading ? 'Äang xá»­ lÃ½...' : 'ÄÄƒng bÃ¡n ngay'}
          </button>
        </div>
      </form>
    </div>
  );
}

