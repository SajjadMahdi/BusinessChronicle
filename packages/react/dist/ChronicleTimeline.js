import { jsx as _jsx, jsxs as _jsxs } from "react/jsx-runtime";
import { useChronicleHistory } from "./hooks.js";
export function ChronicleTimeline({ entity, clientOptions }) {
    const { entries, loading, error } = useChronicleHistory(entity, clientOptions);
    if (loading) {
        return _jsx("div", { className: "bc-timeline bc-timeline--loading", children: "Loading chronicle history\u2026" });
    }
    if (error) {
        return _jsx("div", { className: "bc-timeline bc-timeline--error", children: error.message });
    }
    if (entries.length === 0) {
        return _jsx("div", { className: "bc-timeline bc-timeline--empty", children: "No history entries." });
    }
    return (_jsx("ol", { className: "bc-timeline", children: entries.map((entry) => (_jsxs("li", { className: "bc-timeline__item", children: [_jsx("time", { dateTime: entry.createdAt, children: entry.createdAt }), _jsx("span", { children: entry.message ?? entry.revisionId })] }, entry.revisionId))) }));
}
