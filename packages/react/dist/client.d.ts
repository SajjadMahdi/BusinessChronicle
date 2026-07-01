import type { ChronicleClientOptions, ChronicleEntry, EntityReference, RevisionComparisonResult, TimelineEntry } from "./types.js";
export declare class ChronicleClient {
    private readonly baseUrl;
    private readonly fetchImpl;
    constructor(options: ChronicleClientOptions);
    getHistory(entity: EntityReference, maxResults?: number, signal?: AbortSignal): Promise<ChronicleEntry[]>;
    compare(target: Record<string, unknown>, signal?: AbortSignal): Promise<RevisionComparisonResult>;
    getTimeline(entity: EntityReference, maxResults?: number, signal?: AbortSignal): Promise<TimelineEntry[]>;
}
export declare function createChronicleClient(options: ChronicleClientOptions): ChronicleClient;
//# sourceMappingURL=client.d.ts.map