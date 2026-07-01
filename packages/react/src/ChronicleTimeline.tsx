import type { ChronicleClientOptions, EntityReference } from "./types.js";
import { useChronicleHistory } from "./hooks.js";

export interface ChronicleTimelineProps {
  entity: EntityReference;
  clientOptions: ChronicleClientOptions;
}

export function ChronicleTimeline({ entity, clientOptions }: ChronicleTimelineProps) {
  const { entries, loading, error } = useChronicleHistory(entity, clientOptions);

  if (loading) {
    return <div className="bc-timeline bc-timeline--loading">Loading chronicle history…</div>;
  }

  if (error) {
    return <div className="bc-timeline bc-timeline--error">{error.message}</div>;
  }

  if (entries.length === 0) {
    return <div className="bc-timeline bc-timeline--empty">No history entries.</div>;
  }

  return (
    <ol className="bc-timeline">
      {entries.map((entry) => (
        <li key={entry.revisionId} className="bc-timeline__item">
          <time dateTime={entry.createdAt}>{entry.createdAt}</time>
          <span>{entry.message ?? entry.revisionId}</span>
        </li>
      ))}
    </ol>
  );
}
