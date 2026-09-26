import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createIncident } from '../api/incidents';
import { extractErrorMessage } from '../api/errors';
import type { IncidentSeverity } from '../types';

export function CreateIncidentPage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [severity, setSeverity] = useState<IncidentSeverity>(2);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const incident = await createIncident({ title, description, severity });
      navigate(`/incidents/${incident.id}`);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>New incident</h1>
      </div>

      <form className="card form" onSubmit={handleSubmit}>
        <label>Title</label>
        <input value={title} onChange={(e) => setTitle(e.target.value)} maxLength={200} required />

        <label>Description</label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={5}
          maxLength={4000}
          required
        />

        <label>Severity</label>
        <select value={severity} onChange={(e) => setSeverity(Number(e.target.value) as IncidentSeverity)}>
          <option value={1}>Sev1 — 4 hour SLA</option>
          <option value={2}>Sev2 — 24 hour SLA</option>
          <option value={3}>Sev3 — 72 hour SLA</option>
        </select>

        {error && <div className="form-error">{error}</div>}

        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Creating...' : 'Create incident'}
        </button>
      </form>
    </div>
  );
}
