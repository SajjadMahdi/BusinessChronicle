import { ChronicleClient } from "./client.js";
import type { ChronicleClientOptions, ChronicleEntry, EntityReference, RevisionComparisonResult, TimelineEntry } from "./types.js";
export declare function useChronicleClient(options: ChronicleClientOptions): ChronicleClient;
export declare function useChronicleHistory(entity: EntityReference, options: ChronicleClientOptions): {
    entries: ChronicleEntry[];
    loading: boolean;
    error: Error | null;
    reload: () => Promise<void>;
};
export declare function useChronicleCompare(target: Record<string, unknown> | null, options: ChronicleClientOptions): {
    result: RevisionComparisonResult | null;
    loading: boolean;
    error: Error | null;
    compare: () => Promise<void>;
};
export declare function useTimeline(entity: EntityReference, options: ChronicleClientOptions): {
    entries: TimelineEntry[];
    loading: boolean;
    error: Error | null;
    reload: () => Promise<void>;
};
//# sourceMappingURL=hooks.d.ts.map