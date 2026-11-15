-- Removes the temporary Intelligence items added by temp_intelligence_nov2026.sql.
-- Child rows (sections, key changes, timeline, etc.) are removed by cascade.
DELETE FROM "IntelligenceContentItems" WHERE "Id"::text LIKE 'c0ffee00-2026-1100-%';
