const defaultFetch = globalThis.fetch.bind(globalThis);
export class ChronicleClient {
    baseUrl;
    fetchImpl;
    constructor(options) {
        this.baseUrl = options.baseUrl.replace(/\/$/, "");
        this.fetchImpl = options.fetch ?? defaultFetch;
    }
    async getHistory(entity, maxResults = 100, signal) {
        const url = `${this.baseUrl}/api/chronicle/entities/${encodeURIComponent(entity.entityType)}/${encodeURIComponent(entity.entityId)}/history?maxResults=${maxResults}`;
        const response = await this.fetchImpl(url, { signal });
        if (!response.ok) {
            throw new Error(`History request failed (${response.status})`);
        }
        return (await response.json());
    }
    async compare(target, signal) {
        const response = await this.fetchImpl(`${this.baseUrl}/api/chronicle/compare`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(target),
            signal,
        });
        if (!response.ok) {
            throw new Error(`Compare request failed (${response.status})`);
        }
        return (await response.json());
    }
    async getTimeline(entity, maxResults = 100, signal) {
        const url = `${this.baseUrl}/api/chronicle/entities/${encodeURIComponent(entity.entityType)}/${encodeURIComponent(entity.entityId)}/timeline?maxResults=${maxResults}`;
        const response = await this.fetchImpl(url, { signal });
        if (!response.ok) {
            throw new Error(`Timeline request failed (${response.status})`);
        }
        return (await response.json());
    }
}
export function createChronicleClient(options) {
    return new ChronicleClient(options);
}
