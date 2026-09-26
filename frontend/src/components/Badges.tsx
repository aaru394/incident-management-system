import { SEVERITY_LABEL, STATUS_LABEL, type IncidentSeverity, type IncidentStatus } from '../types';

export function SeverityBadge({ severity }: { severity: IncidentSeverity }) {
  return <span className={`badge severity-${severity}`}>{SEVERITY_LABEL[severity]}</span>;
}

export function StatusBadge({ status }: { status: IncidentStatus }) {
  return <span className={`badge status-${status}`}>{STATUS_LABEL[status]}</span>;
}

export function SlaBadge({ breached }: { breached: boolean }) {
  if (!breached) return null;
  return <span className="badge sla-breached">SLA breached</span>;
}
