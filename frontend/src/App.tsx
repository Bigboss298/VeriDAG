import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Dashboard } from './pages/Dashboard';
import { UploadData } from './pages/UploadData';
import { AccessControl } from './pages/AccessControl';
import { VerifyData } from './pages/VerifyData';
import { AuditLogs } from './pages/AuditLogs';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="upload" element={<UploadData />} />
          <Route path="access-control" element={<AccessControl />} />
          <Route path="verify" element={<VerifyData />} />
          <Route path="audit-logs" element={<AuditLogs />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
