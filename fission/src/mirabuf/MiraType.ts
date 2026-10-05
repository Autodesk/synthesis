/**
 * Kind of Mirabuf assembly.
 *
 * Kept in its own leaf module: `MirabufLoader` initializes its storage backend with a
 * top-level `await`, so importing this enum from there makes the importer wait on that
 * async module. Modules that need only the enum at evaluation time (e.g. the tour step
 * table) would otherwise observe it as `undefined` when an import cycle is involved.
 *
 * The numeric values are persisted in the localStorage asset cache - do not renumber.
 */
export enum MiraType {
    ROBOT = 1,
    FIELD,
}
