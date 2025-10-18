import { useEffect } from 'react';
import { useWalletStore } from '../stores/wallet-store';
import { useInstitutionStore } from '../stores/institution-store';
import { useDataStore } from '../stores/data-store';
import { useAuditStore } from '../stores/audit-store';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { Shield, Upload, CheckCircle, Users, AlertCircle } from 'lucide-react';

export const Dashboard = () => {
  const { address, isConnected } = useWalletStore();
  const { currentInstitution, getInstitutionByWallet } = useInstitutionStore();
  const { records, getInstitutionRecords } = useDataStore();
  const { statistics, recentLogs, getStatistics, getRecentLogs } = useAuditStore();

  useEffect(() => {
    if (address) {
      getInstitutionByWallet(address);
      getInstitutionRecords(address);
      getStatistics();
      getRecentLogs(10);
    }
  }, [address]);

  if (!isConnected) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <Shield className="text-primary-500 mb-4" size={64} />
        <h2 className="text-2xl font-bold text-white mb-2">Welcome to DataTrust Nexus</h2>
        <p className="text-gray-400 text-center max-w-md">
          Connect your wallet to access the secure data exchange platform
        </p>
      </div>
    );
  }

  const COLORS = ['#0ea5e9', '#06b6d4', '#8b5cf6', '#ec4899', '#f59e0b'];

  const actionTypeData = statistics?.actionTypeCounts
    ? Object.entries(statistics.actionTypeCounts).map(([name, value]) => ({
        name: name.replace('_', ' '),
        value,
      }))
    : [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-white">Dashboard</h1>
        {currentInstitution && (
          <p className="text-gray-400 mt-1">
            Welcome, {currentInstitution.name}
          </p>
        )}
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          icon={Upload}
          label="Total Uploads"
          value={statistics?.totalUploads || 0}
          color="bg-blue-500"
        />
        <StatCard
          icon={CheckCircle}
          label="Verifications"
          value={statistics?.totalVerifications || 0}
          color="bg-green-500"
        />
        <StatCard
          icon={Shield}
          label="Access Grants"
          value={statistics?.totalAccessGrants || 0}
          color="bg-purple-500"
        />
        <StatCard
          icon={Users}
          label="Institutions"
          value={statistics?.totalInstitutions || 0}
          color="bg-pink-500"
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Action Types Distribution */}
        <div className="bg-slate-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Activity Distribution</h3>
          {actionTypeData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={actionTypeData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {actionTypeData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">No activity data available</p>
          )}
        </div>

        {/* Verification Success Rate */}
        <div className="bg-slate-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Verification Statistics</h3>
          {statistics && statistics.totalVerifications > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart
                data={[
                  {
                    name: 'Verifications',
                    Successful: statistics.successfulVerifications,
                    Failed: statistics.failedVerifications,
                  },
                ]}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
                <XAxis dataKey="name" stroke="#94a3b8" />
                <YAxis stroke="#94a3b8" />
                <Tooltip
                  contentStyle={{ backgroundColor: '#1e293b', border: 'none' }}
                  labelStyle={{ color: '#fff' }}
                />
                <Legend />
                <Bar dataKey="Successful" fill="#10b981" />
                <Bar dataKey="Failed" fill="#ef4444" />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-gray-400 text-center py-10">No verification data available</p>
          )}
        </div>
      </div>

      {/* Recent Activity */}
      <div className="bg-slate-800 rounded-lg p-6">
        <h3 className="text-lg font-semibold text-white mb-4">Recent Activity</h3>
        {recentLogs.length > 0 ? (
          <div className="space-y-3">
            {recentLogs.slice(0, 5).map((log) => (
              <div
                key={log.id}
                className="flex items-start gap-3 p-3 bg-slate-700 rounded-lg"
              >
                <div className={`p-2 rounded-lg ${log.success ? 'bg-green-500/20' : 'bg-red-500/20'}`}>
                  {log.success ? (
                    <CheckCircle className="text-green-500" size={20} />
                  ) : (
                    <AlertCircle className="text-red-500" size={20} />
                  )}
                </div>
                <div className="flex-1">
                  <p className="text-sm text-white font-medium">
                    {log.actionType.replace(/_/g, ' ')}
                  </p>
                  <p className="text-xs text-gray-400">{log.actionDetails}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    {new Date(log.timestamp).toLocaleString()}
                  </p>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-400 text-center py-10">No recent activity</p>
        )}
      </div>

      {/* My Data Records */}
      <div className="bg-slate-800 rounded-lg p-6">
        <h3 className="text-lg font-semibold text-white mb-4">My Data Records</h3>
        {records.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="text-left text-sm text-gray-400 border-b border-slate-700">
                  <th className="pb-3">File Name</th>
                  <th className="pb-3">Category</th>
                  <th className="pb-3">Uploaded</th>
                  <th className="pb-3">Status</th>
                </tr>
              </thead>
              <tbody className="text-sm">
                {records.slice(0, 5).map((record) => (
                  <tr key={record.recordId} className="border-b border-slate-700">
                    <td className="py-3 text-white">{record.fileName}</td>
                    <td className="py-3 text-gray-400">{record.category}</td>
                    <td className="py-3 text-gray-400">
                      {new Date(record.uploadedAt).toLocaleDateString()}
                    </td>
                    <td className="py-3">
                      <span
                        className={`px-2 py-1 rounded text-xs ${
                          record.isActive
                            ? 'bg-green-500/20 text-green-400'
                            : 'bg-gray-500/20 text-gray-400'
                        }`}
                      >
                        {record.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-400 text-center py-10">No data records found</p>
        )}
      </div>
    </div>
  );
};

interface StatCardProps {
  icon: React.ElementType;
  label: string;
  value: number;
  color: string;
}

const StatCard = ({ icon: Icon, label, value, color }: StatCardProps) => (
  <div className="bg-slate-800 rounded-lg p-6">
    <div className="flex items-center gap-4">
      <div className={`p-3 rounded-lg ${color}`}>
        <Icon className="text-white" size={24} />
      </div>
      <div>
        <p className="text-sm text-gray-400">{label}</p>
        <p className="text-2xl font-bold text-white">{value}</p>
      </div>
    </div>
  </div>
);

