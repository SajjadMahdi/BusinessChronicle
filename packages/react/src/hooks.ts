import { useCallback, useEffect, useMemo, useState } from "react";
import { ChronicleClient, createChronicleClient } from "./client.js";
import type {
  ChronicleClientOptions,
  ChronicleEntry,
  EntityReference,
  RevisionComparisonResult,
  TimelineEntry,
} from "./types.js";

export function useChronicleClient(options: ChronicleClientOptions): ChronicleClient {
  return useMemo(() => createChronicleClient(options), [options.baseUrl, options.fetch]);
}

export function useChronicleHistory(entity: EntityReference, options: ChronicleClientOptions) {
  const client = useChronicleClient(options);
  const [entries, setEntries] = useState<ChronicleEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const reload = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setEntries(await client.getHistory(entity));
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
    } finally {
      setLoading(false);
    }
  }, [client, entity.entityId, entity.entityType]);

  useEffect(() => {
    void reload();
  }, [reload]);

  return { entries, loading, error, reload };
}

export function useChronicleCompare(
  target: Record<string, unknown> | null,
  options: ChronicleClientOptions,
) {
  const client = useChronicleClient(options);
  const [result, setResult] = useState<RevisionComparisonResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const compare = useCallback(async () => {
    if (!target) {
      return;
    }

    setLoading(true);
    setError(null);
    try {
      setResult(await client.compare(target));
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
    } finally {
      setLoading(false);
    }
  }, [client, target]);

  return { result, loading, error, compare };
}

export function useTimeline(entity: EntityReference, options: ChronicleClientOptions) {
  const client = useChronicleClient(options);
  const [entries, setEntries] = useState<TimelineEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const reload = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setEntries(await client.getTimeline(entity));
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
    } finally {
      setLoading(false);
    }
  }, [client, entity.entityId, entity.entityType]);

  useEffect(() => {
    void reload();
  }, [reload]);

  return { entries, loading, error, reload };
}
