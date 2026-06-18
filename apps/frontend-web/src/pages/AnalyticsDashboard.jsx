import { useMemo } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { useHostDashboard } from '../hooks/useHostDashboard';
import './AnalyticsDashboard.css';

const moneyFormatter = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const dateFormatter = new Intl.DateTimeFormat('es-AR', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

const statusMeta = {
  1: { label: 'Confirmada', tone: 'confirmed' },
  2: { label: 'Cancelada', tone: 'cancelled' },
  3: { label: 'Completada', tone: 'completed' },
  confirmed: { label: 'Confirmada', tone: 'confirmed' },
  confirmada: { label: 'Confirmada', tone: 'confirmed' },
  active: { label: 'Activa', tone: 'confirmed' },
  activa: { label: 'Activa', tone: 'confirmed' },
  cancelled: { label: 'Cancelada', tone: 'cancelled' },
  canceled: { label: 'Cancelada', tone: 'cancelled' },
  cancelada: { label: 'Cancelada', tone: 'cancelled' },
  completed: { label: 'Completada', tone: 'completed' },
  completada: { label: 'Completada', tone: 'completed' },
  pending: { label: 'Pendiente', tone: 'pending' },
  pendiente: { label: 'Pendiente', tone: 'pending' },
};

function formatMoney(value) {
  const amount = Number(value ?? 0);
  return `$ ${moneyFormatter.format(Number.isFinite(amount) ? amount : 0)}`;
}

function formatDate(value) {
  if (!value) return '-';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? String(value) : dateFormatter.format(date);
}

function formatShortDate(value) {
  if (!value) return '-';
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? String(value)
    : date.toLocaleDateString('es-AR', { day: '2-digit', month: 'short' });
}

function toArray(value) {
  if (Array.isArray(value)) return value;
  if (value === undefined || value === null || value === '') return [];
  return [value];
}

function getStatusMeta(status) {
  const key = String(status ?? '').trim().toLowerCase();
  return statusMeta[key] ?? { label: status ? String(status) : 'Pendiente', tone: 'default' };
}

function normalizeDashboard(data) {
  const popularWorkspaces = toArray(data?.PopularWorkspaces);
  const recentActivity = toArray(data?.RecentActivity);
  const dailyReservationsChart = toArray(data?.DailyReservationsChart);
  const revenueByWorkspaceChart = toArray(data?.RevenueByWorkspaceChart);

  return {
    totalRevenue: data?.TotalRevenue ?? 0,
    activeReservationsCount: data?.ActiveReservationsCount ?? 0,
    totalWorkspacesCount: data?.TotalWorkspacesCount ?? 0,
    popularWorkspaces: popularWorkspaces
      .map((workspace) => (
        typeof workspace === 'string'
          ? workspace
          : workspace?.WorkspaceName ?? workspace?.Name
      ))
      .filter(Boolean),
    recentActivity: recentActivity.map((reservation, index) => ({
      id: reservation?.Id ?? index,
      workspaceName: reservation?.WorkspaceName ?? '-',
      createdAt: reservation?.CreatedAt,
      status: reservation?.Status,
    })),
    dailyReservationsChart: dailyReservationsChart.map((item) => ({
      date: item?.Date ?? '',
      count: Number(item?.Count ?? 0),
    })),
    revenueByWorkspaceChart: revenueByWorkspaceChart.map((item) => ({
      workspaceName: item?.WorkspaceName ?? '-',
      totalRevenue: Number(item?.TotalRevenue ?? 0),
    })),
  };
}

function hasPositiveValue(data, key) {
  return data.some((item) => Number(item[key] ?? 0) > 0);
}

function DashboardSkeleton() {
  return (
    <section className="analytics-dashboard page-container" aria-label="Cargando dashboard">
      <header className="analytics-dashboard__header">
        <div>
          <div className="analytics-skeleton analytics-skeleton--eyebrow" />
          <div className="analytics-skeleton analytics-skeleton--title" />
          <div className="analytics-skeleton analytics-skeleton--subtitle" />
        </div>
      </header>

      <section className="analytics-dashboard__metrics" aria-label="Cargando métricas">
        {Array.from({ length: 4 }).map((_, index) => (
          <article className="analytics-card analytics-card--skeleton" key={index}>
            <div className="analytics-skeleton analytics-skeleton--label" />
            <div className="analytics-skeleton analytics-skeleton--value" />
            <div className="analytics-skeleton analytics-skeleton--hint" />
          </article>
        ))}
      </section>

      <section className="analytics-panel" aria-label="Cargando actividad reciente">
        <div className="analytics-panel__header">
          <div className="analytics-skeleton analytics-skeleton--panel-title" />
          <div className="analytics-skeleton analytics-skeleton--hint" />
        </div>
        <div className="analytics-table-skeleton">
          {Array.from({ length: 5 }).map((_, index) => (
            <div className="analytics-table-skeleton__row" key={index}>
              <div className="analytics-skeleton analytics-skeleton--cell" />
              <div className="analytics-skeleton analytics-skeleton--cell" />
              <div className="analytics-skeleton analytics-skeleton--badge" />
            </div>
          ))}
        </div>
      </section>

      <section className="analytics-charts" aria-label="Cargando gráficos">
        {Array.from({ length: 2 }).map((_, index) => (
          <article className="analytics-chart-card" key={index}>
            <div className="analytics-panel__header">
              <div className="analytics-skeleton analytics-skeleton--panel-title" />
              <div className="analytics-skeleton analytics-skeleton--hint" />
            </div>
            <div className="analytics-chart-skeleton">
              <div className="analytics-skeleton analytics-skeleton--chart" />
            </div>
          </article>
        ))}
      </section>
    </section>
  );
}

function MetricCard({ label, value }) {
  return (
    <article className="analytics-card">
      <p>{label}</p>
      <strong>{value}</strong>
    </article>
  );
}

function RecentActivityTable({ activity }) {
  if (activity.length === 0) {
    return (
      <div className="analytics-empty">
        No hay actividad reciente
      </div>
    );
  }

  return (
    <div className="analytics-table-wrap">
      <table className="analytics-table">
        <thead>
          <tr>
            <th>Oficina</th>
            <th>Fecha</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          {activity.map((reservation) => {
            const meta = getStatusMeta(reservation.status);

            return (
              <tr key={reservation.id}>
                <td>{reservation.workspaceName}</td>
                <td>{formatDate(reservation.createdAt)}</td>
                <td>
                  <span className={`analytics-status analytics-status--${meta.tone}`}>
                    {meta.label}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

function ChartEmptyState() {
  return (
    <div className="analytics-chart-empty">
      Aún no tienes datos para graficar
    </div>
  );
}

function ReservationsLineChart({ data }) {
  if (!hasPositiveValue(data, 'count')) return <ChartEmptyState />;

  return (
    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={data} margin={{ top: 12, right: 18, bottom: 8, left: 0 }}>
        <CartesianGrid stroke="#dbe0e8" strokeDasharray="4 4" vertical={false} />
        <XAxis
          dataKey="date"
          tickFormatter={formatShortDate}
          tickLine={false}
          axisLine={false}
          minTickGap={18}
        />
        <YAxis allowDecimals={false} tickLine={false} axisLine={false} width={36} />
        <Tooltip
          formatter={(value) => [Number(value).toLocaleString('es-AR'), 'Reservas']}
          labelFormatter={(label) => `Fecha: ${formatShortDate(label)}`}
        />
        <Line
          type="monotone"
          dataKey="count"
          stroke="var(--color-primary)"
          strokeWidth={3}
          dot={{ r: 3 }}
          activeDot={{ r: 5 }}
          name="Reservas"
        />
      </LineChart>
    </ResponsiveContainer>
  );
}

function RevenueBarChart({ data }) {
  if (!hasPositiveValue(data, 'totalRevenue')) return <ChartEmptyState />;

  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data} margin={{ top: 12, right: 18, bottom: 8, left: 0 }}>
        <CartesianGrid stroke="#dbe0e8" strokeDasharray="4 4" vertical={false} />
        <XAxis
          dataKey="workspaceName"
          tickLine={false}
          axisLine={false}
          interval={0}
          minTickGap={10}
          tickFormatter={(value) => (
            String(value).length > 14 ? `${String(value).slice(0, 14)}…` : value
          )}
        />
        <YAxis
          tickLine={false}
          axisLine={false}
          width={58}
          tickFormatter={(value) => `$ ${Number(value).toLocaleString('en-US')}`}
        />
        <Tooltip
          formatter={(value) => [formatMoney(value), 'Ingresos']}
          labelFormatter={(label) => `Oficina: ${label}`}
        />
        <Bar dataKey="totalRevenue" fill="var(--color-accent)" radius={[8, 8, 0, 0]} name="Ingresos" />
      </BarChart>
    </ResponsiveContainer>
  );
}

function AnalyticsDashboard() {
  const { data, isLoading, isError } = useHostDashboard();
  const dashboard = useMemo(() => normalizeDashboard(data), [data]);
  const popularWorkspaces = dashboard.popularWorkspaces.length > 0
    ? dashboard.popularWorkspaces.join(', ')
    : 'Sin datos';

  if (isLoading) return <DashboardSkeleton />;

  return (
    <section className="analytics-dashboard page-container">
      <header className="analytics-dashboard__header">
        <div>
          <p className="analytics-dashboard__eyebrow">Panel de empresa</p>
          <h1 className="analytics-dashboard__title">Dashboard</h1>
          <p className="analytics-dashboard__subtitle">
            Monitorea ingresos, reservas y actividad reciente de tus espacios.
          </p>
        </div>
      </header>

      {isError && (
        <div className="analytics-alert" role="alert">
          No se pudo cargar la información del dashboard.
        </div>
      )}

      <section className="analytics-dashboard__metrics" aria-label="Resumen de rendimiento">
        <MetricCard label="Ingresos Totales" value={formatMoney(dashboard.totalRevenue)} />
        <MetricCard label="Reservas Activas" value={dashboard.activeReservationsCount} />
        <MetricCard label="Oficinas Totales" value={dashboard.totalWorkspacesCount} />
        <MetricCard label="Oficinas Populares" value={popularWorkspaces} />
      </section>

      <section className="analytics-panel">
        <header className="analytics-panel__header">
          <div>
            <h2>Actividad reciente</h2>
            <p>Últimas reservas registradas y su estado actual.</p>
          </div>
        </header>
        <RecentActivityTable activity={dashboard.recentActivity} />
      </section>

      <section className="analytics-charts" aria-label="Gráficos analíticos">
        <article className="analytics-chart-card">
          <header className="analytics-panel__header">
            <div>
              <h2>Reservas por día</h2>
              <p>Volumen de reservas de los últimos 30 días.</p>
            </div>
          </header>
          <div className="analytics-chart">
            <ReservationsLineChart data={dashboard.dailyReservationsChart} />
          </div>
        </article>

        <article className="analytics-chart-card">
          <header className="analytics-panel__header">
            <div>
              <h2>Ingresos por oficina</h2>
              <p>Rentabilidad acumulada por espacio publicado.</p>
            </div>
          </header>
          <div className="analytics-chart">
            <RevenueBarChart data={dashboard.revenueByWorkspaceChart} />
          </div>
        </article>
      </section>
    </section>
  );
}

export default AnalyticsDashboard;
