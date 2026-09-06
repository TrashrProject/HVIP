# ParadiseRP Catalogue V4

The active catalogue taxonomy must merge the existing player catalogue with Paradise additions instead of keeping separate novelty/building catalogues.

Canonical migrations:
- `migrations/20260906_paradise_catalogue_taxonomy_v4_global.sql`
- `migrations/20260906_paradise_catalogue_taxonomy_v4_global_legacy.sql`

Expected structure:
- 24 top-level semantic groups under `9967200`.
- Smaller nested pages for high-volume groups (building, water/nature, furniture, kitchen, technology, transport, leisure and RP specialties).
- Existing visible player catalogue pages are collected as sources, their offers are reassigned by furniture semantics, then source pages are hidden to avoid duplicates.
- Furniture definitions are not deleted or rewritten by the taxonomy migration.

Validation metrics are available through `migrations/20260906_paradise_catalogue_taxonomy_v4_validation.sql`.
