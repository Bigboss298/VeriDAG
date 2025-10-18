import { useState, useEffect } from 'react';
import { useWalletStore } from '../stores/wallet-store';
import { Wallet, LogOut } from 'lucide-react';

declare global {
  interface Window {
    ethereum?: any;
  }
}

export const WalletConnect = () => {
  const { address, isConnected, setWalletAddress, setConnected, disconnect } = useWalletStore();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    checkIfWalletIsConnected();
    
    if (window.ethereum) {
      window.ethereum.on('accountsChanged', handleAccountsChanged);
      window.ethereum.on('chainChanged', () => window.location.reload());
    }

    return () => {
      if (window.ethereum) {
        window.ethereum.removeListener('accountsChanged', handleAccountsChanged);
      }
    };
  }, []);

  const checkIfWalletIsConnected = async () => {
    try {
      if (!window.ethereum) return;

      const accounts = await window.ethereum.request({ method: 'eth_accounts' });
      if (accounts.length > 0) {
        setWalletAddress(accounts[0]);
        setConnected(true);
      }
    } catch (err) {
      console.error('Error checking wallet connection:', err);
    }
  };

  const handleAccountsChanged = (accounts: string[]) => {
    if (accounts.length === 0) {
      disconnect();
    } else {
      setWalletAddress(accounts[0]);
      setConnected(true);
    }
  };

  const connectWallet = async () => {
    try {
      setIsLoading(true);
      setError(null);

      if (!window.ethereum) {
        setError('MetaMask is not installed. Please install it to use this app.');
        setIsLoading(false);
        return;
      }

      const accounts = await window.ethereum.request({
        method: 'eth_requestAccounts',
      });

      if (accounts.length > 0) {
        setWalletAddress(accounts[0]);
        setConnected(true);
      }

      setIsLoading(false);
    } catch (err: any) {
      setError(err.message || 'Failed to connect wallet');
      setIsLoading(false);
    }
  };

  const handleDisconnect = () => {
    disconnect();
  };

  if (isConnected && address) {
    return (
      <div className="flex items-center gap-3">
        <div className="bg-slate-700 px-4 py-2 rounded-lg">
          <p className="text-sm text-gray-300">Connected</p>
          <p className="text-xs font-mono text-primary-400">
            {address.slice(0, 6)}...{address.slice(-4)}
          </p>
        </div>
        <button
          onClick={handleDisconnect}
          className="flex items-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition"
        >
          <LogOut size={18} />
          <span>Disconnect</span>
        </button>
      </div>
    );
  }

  return (
    <div>
      <button
        onClick={connectWallet}
        disabled={isLoading}
        className="flex items-center gap-2 px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <Wallet size={20} />
        <span>{isLoading ? 'Connecting...' : 'Connect Wallet'}</span>
      </button>
      
      {error && (
        <p className="mt-2 text-sm text-red-400">{error}</p>
      )}
    </div>
  );
};

