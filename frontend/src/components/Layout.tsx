import { Link, Outlet, useLocation } from 'react-router-dom';
import { WalletConnect } from './WalletConnect';
import { Shield, LayoutDashboard, Upload, Lock, CheckCircle, FileText } from 'lucide-react';

export const Layout = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Dashboard', icon: LayoutDashboard },
    { path: '/upload', label: 'Upload Data', icon: Upload },
    { path: '/access-control', label: 'Access Control', icon: Lock },
    { path: '/verify', label: 'Verify Data', icon: CheckCircle },
    { path: '/audit-logs', label: 'Audit Logs', icon: FileText },
  ];

  return (
    <div className="min-h-screen bg-slate-900">
      {/* Header */}
      <header className="bg-slate-800 border-b border-slate-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center gap-3">
              <Shield className="text-primary-500" size={32} />
              <div>
                <h1 className="text-xl font-bold text-white">DataTrust Nexus</h1>
                <p className="text-xs text-gray-400">Secure Data Exchange Platform</p>
              </div>
            </div>
            
            <WalletConnect />
          </div>
        </div>
      </header>

      {/* Navigation */}
      <nav className="bg-slate-800 border-b border-slate-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex space-x-4 overflow-x-auto py-2">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path;
              
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg transition whitespace-nowrap ${
                    isActive
                      ? 'bg-primary-600 text-white'
                      : 'text-gray-300 hover:bg-slate-700 hover:text-white'
                  }`}
                >
                  <Icon size={18} />
                  <span className="text-sm font-medium">{item.label}</span>
                </Link>
              );
            })}
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="bg-slate-800 border-t border-slate-700 mt-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <p className="text-center text-sm text-gray-400">
            © 2024 DataTrust Nexus. ISO 27001 & NIST SP 800-53 Compliant. Built on BlockDAG Chain.
          </p>
        </div>
      </footer>
    </div>
  );
};

