import { useEffect, useState, type FormEvent } from 'react';
import { useParams } from 'react-router-dom';
import { addComment, assignIncident, changeIncidentStatus, fetchIncident } from '../api/incidents';
import { extractErrorMessage } from '../api/errors';
import { SeverityBadge, SlaBadge, StatusBadge } from '../components/Badges';
import { useAuth } from '../context/AuthContext';
import { STATUS_LABEL, type Incident, type IncidentStatus } from '../types';

const NEXT_STATUS: Record<IncidentStatus, IncidentStatus[]> = {
  1: [2],
  2: [3, 1],
  3: [4, 2],
  4: [],
};

export function IncidentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [incident, setIncident] = useState<Incident | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);

  function load() {
    if (!id) return;
    setLoading(true);
    fetchIncident(id)
      .then(setIncident)
      .catch((err) => setError(extractErrorMessage(err)))
      .finally(() => setLoading(false));
  }

  useEffect(load, [id]);

  async function handleAssignToMe() {
    if (!id || !user) return;
    setActionError(null);
    setBusy(true);
    try {
      const updated = await assignIncident(id, user.userId);
      setIncident(updated);
    } catch (err) {
      setActionError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function handleStatusChange(status: IncidentStatus) {
    if (!id) return;
    setActionError(null);
    setBusy(true);
    try {
      const updated = await changeIncidentStatus(id, status);
      setIncident(updated);
    } catch (err) {
      setActionError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function handleCommentSubmit(e: FormEvent) {
    e.preventDefault();
    if (!id || !comment.trim()) return;
    setActionError(null);
    setBusy(true);
    try {
      await addComment(id, comment);
      setComment('');
      load();
    } catch (err) {
      setActionError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <div className="page hint">Loading...</div>;
  if (error) return <div className="page form-error">{error}</div>;
  if (!incident) return null;

  return (
    <div className="page">
      <div className="page-header">
        <h1>{incident.title}</h1>
        <div className="incident-row-badges">
          <SeverityBadge severity={incident.severity} />
          <StatusBadge status={incident.status} />
          <SlaBadge breached={incident.isSlaBreached} />
        </div>
      </div>

      <div className="detail-grid">
        <div className="card">
          <h2>Description</h2>
          <p>{incident.description}</p>

          <dl className="meta-list">
            <dt>Reporter</dt>
            <dd>{incident.reporterName}</dd>
            <dt>Assigned to</dt>
            <dd>{incident.assignedToName ?? 'Unassigned'}</dd>
            <dt>SLA deadline</dt>
            <dd>{new Date(incident.slaDeadline).toLocaleString()}</dd>
            <dt>Created</dt>
            <dd>{new Date(incident.createdAt).toLocaleString()}</dd>
          </dl>

          {actionError && <div className="form-error">{actionError}</div>}

          <div className="actions">
            {!incident.assignedToId && (
              <button className="btn-secondary" disabled={busy} onClick={handleAssignToMe}>
                Assign to me
              </button>
            )}
            {NEXT_STATUS[incident.status].map((next) => (
              <button key={next} className="btn-secondary" disabled={busy} onClick={() => handleStatusChange(next)}>
                Move to {STATUS_LABEL[next]}
              </button>
            ))}
          </div>
        </div>

        <div className="card">
          <h2>Status history</h2>
          {(incident.statusHistory ?? []).length === 0 && <p className="hint">No transitions yet.</p>}
          <ul className="timeline">
            {(incident.statusHistory ?? []).map((h, i) => (
              <li key={i}>
                <span>
                  {STATUS_LABEL[h.fromStatus]} → {STATUS_LABEL[h.toStatus]}
                </span>
                <span className="hint">
                  by {h.changedByName} · {new Date(h.changedAt).toLocaleString()}
                </span>
              </li>
            ))}
          </ul>
        </div>

        <div className="card">
          <h2>Comments</h2>
          <ul className="comment-list">
            {(incident.comments ?? []).map((c) => (
              <li key={c.id}>
                <div className="comment-meta">
                  <strong>{c.userName}</strong>
                  <span className="hint">{new Date(c.createdAt).toLocaleString()}</span>
                </div>
                <p>{c.message}</p>
              </li>
            ))}
          </ul>

          <form onSubmit={handleCommentSubmit} className="comment-form">
            <textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Add a comment..."
              rows={2}
              maxLength={2000}
            />
            <button type="submit" className="btn-primary" disabled={busy || !comment.trim()}>
              Post
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
