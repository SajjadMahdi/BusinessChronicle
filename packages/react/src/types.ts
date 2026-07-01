export interface ChronicleClientOptions {
  baseUrl: string;
  fetch?: typeof fetch;
}

export interface EntityReference {
  entityType: string;
  entityId: string;
}

export interface ChronicleEntry {
  revisionId: string;
  entityType: string;
  entityId: string;
  message?: string;
  createdAt: string;
}

export interface RevisionComparisonResult {
  entityType: string;
  entityId: string;
  changes: Array<{ path: string; kind: string }>;
}

export interface TimelineEntry {
  id: string;
  entityType: string;
  entityId: string;
  summary: string;
  occurredAt: string;
}
