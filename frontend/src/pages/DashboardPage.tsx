import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { fetchIncidents } from '../api/incidents';
import { extractErrorMessage } from '../api/errors';
import { SeverityBadge, SlaBadge, StatusBadge } from '../components/Badges';
import type { Incident, IncidentSeverity, IncidentStatus } from '../types';

export function DashboardPage() {
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [status, setStatus] = useState<string>('');
  const [severity, setSeverity] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    fetchIncidents({
      status: status ? (Number(status) as IncidentStatus) : undefined,
      severity: severity ? (Number(severity) as IncidentSeverity) : undefined,
      page: 1,
      pageSize: 50,
    })
      .then((result) => {
        if (!cancelled) setIncidents(result.items);
      })
      .catch((err) => {
        if (!cancelled) setError(extractErrorMessage(err));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [status, severity]);

  return (
    <div className="page">
      <div className="page-header">
        <h1>Incidents</h1>
        <Link to="/incidents/new" className="btn-primary">
          New incident
        </Link>
      </div>

      <div className="filters">
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          <option value="1">Open</option>
          <option value="2">In progress</option>
          <option value="3">Resolved</option>
          <option value="4">Closed</option>
        </select>
        <select value={severity} onChange={(e) => setSeverity(e.target.value)}>
          <option value="">All severities</option>
          <option value="1">Sev1</option>
          <option value="2">Sev2</option>
          <option value="3">Sev3</option>
        </select>
      </div>

      {loading && <p className="hint">Loading...</p>}
      {error && <div className="form-error">{error}</div>}

      {!loading && !error && incidents.length === 0 && (
        <p className="hint">No incidents yet. Create one to get started.</p>
      )}

      <div className="incident-list">
        {incidents.map((incident) => (
          <Link to={`/incidents/${incident.id}`} key={incident.id} className="incident-row">
            <div className="incident-row-main">
              <span className="incident-title">{incident.title}</span>
              <span className="incident-meta">
                Reported by {incident.reporterName}
                {incident.assignedToName ? ` · assigned to ${incident.assignedToName}` : ' · unassigned'}
              </span>
            </div>
            <div className="incident-row-badges">
              <SeverityBadge severity={incident.severity} />
              <StatusBadge status={incident.status} />
              <SlaBadge breached={incident.isSlaBreached} />
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
