import type {
  ChronicleClientOptions,
  ChronicleEntry,
  EntityReference,
  RevisionComparisonResult,
  TimelineEntry,
} from "./types.js";

const defaultFetch = globalThis.fetch.bind(globalThis);

export class ChronicleClient {
  private readonly baseUrl: string;
  private readonly fetchImpl: typeof fetch;

  constructor(options: ChronicleClientOptions) {
    this.baseUrl = options.baseUrl.replace(/\/$/, "");
    this.fetchImpl = options.fetch ?? defaultFetch;
  }

  async getHistory(
    entity: EntityReference,
    maxResults = 100,
    signal?: AbortSignal,
  ): Promise<ChronicleEntry[]> {
    const url = `${this.baseUrl}/api/chronicle/entities/${encodeURIComponent(entity.entityType)}/${encodeURIComponent(entity.entityId)}/history?maxResults=${maxResults}`;
    const response = await this.fetchImpl(url, { signal });
    if (!response.ok) {
      throw new Error(`History request failed (${response.status})`);
    }

    return (await response.json()) as ChronicleEntry[];
  }

  async compare(
    target: Record<string, unknown>,
    signal?: AbortSignal,
  ): Promise<RevisionComparisonResult> {
    const response = await this.fetchImpl(`${this.baseUrl}/api/chronicle/compare`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(target),
      signal,
    });
    if (!response.ok) {
      throw new Error(`Compare request failed (${response.status})`);
    }

    return (await response.json()) as RevisionComparisonResult;
  }

  async getTimeline(
    entity: EntityReference,
    maxResults = 100,
    signal?: AbortSignal,
  ): Promise<TimelineEntry[]> {
    const url = `${this.baseUrl}/api/chronicle/entities/${encodeURIComponent(entity.entityType)}/${encodeURIComponent(entity.entityId)}/timeline?maxResults=${maxResults}`;
    const response = await this.fetchImpl(url, { signal });
    if (!response.ok) {
      throw new Error(`Timeline request failed (${response.status})`);
    }

    return (await response.json()) as TimelineEntry[];
  }
}

export function createChronicleClient(options: ChronicleClientOptions): ChronicleClient {
  return new ChronicleClient(options);
}
