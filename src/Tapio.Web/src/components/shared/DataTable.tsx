import { useMemo, useState } from 'react';
import { EmptyState, LoadingState } from './StateViews';

export type SortDir = 'asc' | 'desc';

export type DataTableColumn<T> = {
  key: string;
  header: string;
  sortable?: boolean;
  render?: (row: T) => React.ReactNode;
  getSortValue?: (row: T) => string | number;
  getSearchValue?: (row: T) => string;
  className?: string;
};

type ServerQuery = {
  page: number;
  pageSize: number;
  search: string;
  sortKey: string;
  sortDir: SortDir;
};

type Props<T> = {
  columns: DataTableColumn<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  loading?: boolean;
  emptyMessage?: string;
  pageSizeOptions?: number[];
  mode?: 'client' | 'server';
  totalCount?: number;
  query?: ServerQuery;
  onQueryChange?: (query: ServerQuery) => void;
  toolbar?: React.ReactNode;
};

function compareValues(a: string | number, b: string | number): number {
  if (typeof a === 'number' && typeof b === 'number') return a - b;
  return String(a).localeCompare(String(b), 'tr', { sensitivity: 'base' });
}

export function DataTable<T>({
  columns,
  rows,
  rowKey,
  loading,
  emptyMessage = 'Kayıt bulunamadı.',
  pageSizeOptions = [10, 20, 50],
  mode = 'client',
  totalCount,
  query,
  onQueryChange,
  toolbar
}: Props<T>) {
  const [clientSearch, setClientSearch] = useState('');
  const [clientSortKey, setClientSortKey] = useState(columns.find(c => c.sortable)?.key ?? columns[0]?.key ?? '');
  const [clientSortDir, setClientSortDir] = useState<SortDir>('desc');
  const [clientPage, setClientPage] = useState(1);
  const [clientPageSize, setClientPageSize] = useState(pageSizeOptions[1] ?? 20);

  const isServer = mode === 'server' && query && onQueryChange;
  const search = isServer ? query.search : clientSearch;
  const sortKey = isServer ? query.sortKey : clientSortKey;
  const sortDir = isServer ? query.sortDir : clientSortDir;
  const page = isServer ? query.page : clientPage;
  const pageSize = isServer ? query.pageSize : clientPageSize;

  const processedRows = useMemo(() => {
    if (isServer) return rows;

    let list = [...rows];
    const term = clientSearch.trim().toLowerCase();
    if (term) {
      list = list.filter(row =>
        columns.some(col => {
          const text = col.getSearchValue?.(row) ?? (col.render ? String(col.render(row) ?? '') : '');
          return text.toLowerCase().includes(term);
        })
      );
    }

    const col = columns.find(c => c.key === clientSortKey);
    if (col?.sortable) {
      list.sort((a, b) => {
        const av = col.getSortValue?.(a) ?? '';
        const bv = col.getSortValue?.(b) ?? '';
        const cmp = compareValues(av, bv);
        return clientSortDir === 'asc' ? cmp : -cmp;
      });
    }

    return list;
  }, [isServer, rows, columns, clientSearch, clientSortKey, clientSortDir]);

  const effectiveTotal = isServer ? (totalCount ?? 0) : processedRows.length;
  const pageCount = Math.max(1, Math.ceil(effectiveTotal / pageSize));
  const safePage = Math.min(page, pageCount);

  const pageRows = useMemo(() => {
    if (isServer) return rows;
    const start = (safePage - 1) * pageSize;
    return processedRows.slice(start, start + pageSize);
  }, [isServer, rows, processedRows, safePage, pageSize]);

  const setSearch = (value: string) => {
    if (isServer) onQueryChange!({ ...query, search: value, page: 1 });
    else {
      setClientSearch(value);
      setClientPage(1);
    }
  };

  const toggleSort = (key: string, sortable?: boolean) => {
    if (!sortable) return;
    const nextDir: SortDir = sortKey === key && sortDir === 'asc' ? 'desc' : 'asc';
    if (isServer) onQueryChange!({ ...query, sortKey: key, sortDir: nextDir, page: 1 });
    else {
      setClientSortKey(key);
      setClientSortDir(nextDir);
      setClientPage(1);
    }
  };

  const setPage = (next: number) => {
    const p = Math.min(Math.max(1, next), pageCount);
    if (isServer) onQueryChange!({ ...query, page: p });
    else setClientPage(p);
  };

  const setPageSize = (size: number) => {
    if (isServer) onQueryChange!({ ...query, pageSize: size, page: 1 });
    else {
      setClientPageSize(size);
      setClientPage(1);
    }
  };

  const from = effectiveTotal === 0 ? 0 : (safePage - 1) * pageSize + 1;
  const to = Math.min(safePage * pageSize, effectiveTotal);

  return (
    <div className="data-table card">
      <div className="data-table-toolbar">
        <input
          className="input data-table-search"
          placeholder="Ara..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          aria-label="Tabloda ara"
        />
        {toolbar}
      </div>

      {loading ? (
        <LoadingState message="Yükleniyor..." />
      ) : pageRows.length === 0 ? (
        <EmptyState message={emptyMessage} />
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                {columns.map(col => (
                  <th key={col.key} className={col.className}>
                    {col.sortable ? (
                      <button type="button" className="table-sort" onClick={() => toggleSort(col.key, col.sortable)}>
                        {col.header}
                        {sortKey === col.key && <span className="table-sort-indicator">{sortDir === 'asc' ? ' ↑' : ' ↓'}</span>}
                      </button>
                    ) : (
                      col.header
                    )}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {pageRows.map(row => (
                <tr key={rowKey(row)}>
                  {columns.map(col => (
                    <td key={col.key} className={col.className}>
                      {col.render ? col.render(row) : null}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="data-table-footer">
        <div className="data-table-range">
          {effectiveTotal > 0 ? `${from}–${to} / ${effectiveTotal} kayıt` : '0 kayıt'}
        </div>
        <div className="data-table-pagination">
          <label className="data-table-page-size">
            Sayfa boyutu
            <select className="select" value={pageSize} onChange={e => setPageSize(Number(e.target.value))}>
              {pageSizeOptions.map(n => (
                <option key={n} value={n}>{n}</option>
              ))}
            </select>
          </label>
          <button type="button" className="btn btn-secondary btn-sm" disabled={safePage <= 1} onClick={() => setPage(safePage - 1)}>
            Önceki
          </button>
          <span className="data-table-page-indicator">{safePage} / {pageCount}</span>
          <button type="button" className="btn btn-secondary btn-sm" disabled={safePage >= pageCount} onClick={() => setPage(safePage + 1)}>
            Sonraki
          </button>
        </div>
      </div>
    </div>
  );
}

export type { ServerQuery as DataTableServerQuery };
