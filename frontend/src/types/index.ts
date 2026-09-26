export type IncidentSeverity = 1 | 2 | 3;
export type IncidentStatus = 1 | 2 | 3 | 4;

export const SEVERITY_LABEL: Record<IncidentSeverity, string> = {
  1: 'Sev1',
  2: 'Sev2',
  3: 'Sev3',
};

export const STATUS_LABEL: Record<IncidentStatus, string> = {
  1: 'Open',
  2: 'In progress',
  3: 'Resolved',
  4: 'Closed',
};

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: string;
  fullName: string;
  role: string;
}

export interface IncidentComment {
  id: string;
  userId: string;
  userName: string;
  message: string;
  createdAt: string;
}

export interface IncidentStatusChange {
  fromStatus: IncidentStatus;
  toStatus: IncidentStatus;
  changedByName: string;
  changedAt: string;
}

export interface Incident {
  id: string;
  title: string;
  description: string;
  severity: IncidentSeverity;
  status: IncidentStatus;
  createdAt: string;
  updatedAt: string;
  slaDeadline: string;
  isSlaBreached: boolean;
  resolvedAt: string | null;
  reporterId: string;
  reporterName: string;
  assignedToId: string | null;
  assignedToName: string | null;
  comments: IncidentComment[];
  statusHistory: IncidentStatusChange[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiError {
  title: string;
  status: number;
  detail: string;
  traceId: string;
}
