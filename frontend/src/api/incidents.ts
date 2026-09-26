import { apiClient } from './client';
import type { Incident, IncidentSeverity, IncidentStatus, PagedResult } from '../types';

export interface IncidentQuery {
  status?: IncidentStatus;
  severity?: IncidentSeverity;
  page?: number;
  pageSize?: number;
}

export async function fetchIncidents(query: IncidentQuery): Promise<PagedResult<Incident>> {
  const response = await apiClient.get<PagedResult<Incident>>('/incidents', { params: query });
  return response.data;
}

export async function fetchIncident(id: string): Promise<Incident> {
  const response = await apiClient.get<Incident>(`/incidents/${id}`);
  return response.data;
}

export async function createIncident(payload: {
  title: string;
  description: string;
  severity: IncidentSeverity;
}): Promise<Incident> {
  const response = await apiClient.post<Incident>('/incidents', payload);
  return response.data;
}

export async function assignIncident(id: string, userId: string): Promise<Incident> {
  const response = await apiClient.post<Incident>(`/incidents/${id}/assign`, { userId });
  return response.data;
}

export async function changeIncidentStatus(id: string, status: IncidentStatus): Promise<Incident> {
  const response = await apiClient.patch<Incident>(`/incidents/${id}/status`, { status });
  return response.data;
}

export async function addComment(id: string, message: string) {
  const response = await apiClient.post(`/incidents/${id}/comments`, { message });
  return response.data;
}
