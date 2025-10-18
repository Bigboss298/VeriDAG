import { useState } from 'react';
import { useWalletStore } from '../stores/wallet-store';
import { useDataStore } from '../stores/data-store';
import { Upload, FileText, CheckCircle, AlertCircle } from 'lucide-react';

export const UploadData = () => {
  const { address, isConnected } = useWalletStore();
  const { uploadData, isLoading } = useDataStore();
  
  const [formData, setFormData] = useState({
    fileName: '',
    fileType: '',
    fileSize: 0,
    category: 'Academic',
    description: '',
    dataHash: '',
    ipfsHash: '',
  });
  
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!address) {
      setError('Please connect your wallet');
      return;
    }

    try {
      setError(null);
      setSuccess(false);
      
      const recordId = `REC-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      await uploadData(
        {
          recordId,
          dataHash: formData.dataHash,
          fileName: formData.fileName,
          fileType: formData.fileType,
          fileSize: formData.fileSize,
          ipfsHash: formData.ipfsHash,
          encryptionAlgorithm: 'AES-256-GCM',
          category: formData.category,
          description: formData.description,
        },
        address
      );
      
      setSuccess(true);
      setFormData({
        fileName: '',
        fileType: '',
        fileSize: 0,
        category: 'Academic',
        description: '',
        dataHash: '',
        ipfsHash: '',
      });
    } catch (err: any) {
      setError(err.message || 'Failed to upload data');
    }
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setFormData((prev) => ({
      ...prev,
      fileName: file.name,
      fileType: file.type,
      fileSize: file.size,
    }));

    // In production, compute actual hash and upload to IPFS
    // For demo, generate mock values
    const mockHash = Array.from({ length: 64 }, () =>
      Math.floor(Math.random() * 16).toString(16)
    ).join('');
    const mockIpfsHash = 'Qm' + Array.from({ length: 44 }, () =>
      Math.floor(Math.random() * 36).toString(36)
    ).join('');

    setFormData((prev) => ({
      ...prev,
      dataHash: mockHash,
      ipfsHash: mockIpfsHash,
    }));
  };

  if (!isConnected) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <Upload className="text-primary-500 mb-4" size={64} />
        <h2 className="text-2xl font-bold text-white mb-2">Upload Data</h2>
        <p className="text-gray-400 text-center max-w-md">
          Please connect your wallet to upload data
        </p>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-white">Upload Data</h1>
        <p className="text-gray-400 mt-1">
          Upload encrypted data to the blockchain with AES-256-GCM encryption
        </p>
      </div>

      {success && (
        <div className="mb-6 p-4 bg-green-500/20 border border-green-500 rounded-lg flex items-start gap-3">
          <CheckCircle className="text-green-500 flex-shrink-0" size={20} />
          <div>
            <p className="text-green-400 font-medium">Data uploaded successfully!</p>
            <p className="text-green-300 text-sm mt-1">
              Your data has been encrypted and stored on the blockchain.
            </p>
          </div>
        </div>
      )}

      {error && (
        <div className="mb-6 p-4 bg-red-500/20 border border-red-500 rounded-lg flex items-start gap-3">
          <AlertCircle className="text-red-500 flex-shrink-0" size={20} />
          <div>
            <p className="text-red-400 font-medium">Upload failed</p>
            <p className="text-red-300 text-sm mt-1">{error}</p>
          </div>
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-slate-800 rounded-lg p-6 space-y-6">
        {/* File Upload */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Select File
          </label>
          <div className="flex items-center justify-center w-full">
            <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-slate-600 border-dashed rounded-lg cursor-pointer hover:bg-slate-700 transition">
              <div className="flex flex-col items-center justify-center pt-5 pb-6">
                <FileText className="text-gray-400 mb-2" size={32} />
                <p className="text-sm text-gray-400">
                  <span className="font-semibold">Click to upload</span> or drag and drop
                </p>
                <p className="text-xs text-gray-500">Any file type supported</p>
              </div>
              <input
                type="file"
                className="hidden"
                onChange={handleFileChange}
                required
              />
            </label>
          </div>
          {formData.fileName && (
            <p className="mt-2 text-sm text-primary-400">Selected: {formData.fileName}</p>
          )}
        </div>

        {/* Category */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Category
          </label>
          <select
            value={formData.category}
            onChange={(e) => setFormData({ ...formData, category: e.target.value })}
            className="w-full px-4 py-2 bg-slate-700 border border-slate-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
            required
          >
            <option value="Academic">Academic</option>
            <option value="Medical">Medical</option>
            <option value="Financial">Financial</option>
            <option value="Legal">Legal</option>
            <option value="Government">Government</option>
            <option value="Other">Other</option>
          </select>
        </div>

        {/* Description */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            className="w-full px-4 py-2 bg-slate-700 border border-slate-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
            rows={3}
            placeholder="Describe the data..."
          />
        </div>

        {/* Technical Details (Read-only) */}
        {formData.dataHash && (
          <div className="space-y-3 p-4 bg-slate-700 rounded-lg">
            <h3 className="text-sm font-medium text-gray-300">Technical Details</h3>
            <div>
              <p className="text-xs text-gray-400 mb-1">SHA-256 Hash</p>
              <p className="text-xs font-mono text-primary-400 break-all">{formData.dataHash}</p>
            </div>
            <div>
              <p className="text-xs text-gray-400 mb-1">IPFS Hash</p>
              <p className="text-xs font-mono text-primary-400 break-all">{formData.ipfsHash}</p>
            </div>
            <div>
              <p className="text-xs text-gray-400 mb-1">File Size</p>
              <p className="text-xs text-white">{(formData.fileSize / 1024).toFixed(2)} KB</p>
            </div>
          </div>
        )}

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading || !formData.fileName}
          className="w-full flex items-center justify-center gap-2 px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Upload size={20} />
          <span>{isLoading ? 'Uploading...' : 'Upload to Blockchain'}</span>
        </button>
      </form>
    </div>
  );
};

