import { useMemo } from 'react';
import {
  Building2,
  CalendarCheck,
  Calendar,
  DollarSign,
  Star,
} from 'lucide-react';
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
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

const USE_MOCK_DASHBOARD = true;

const MOCK_HOST_DASHBOARD = {
  TotalRevenue: 148750.5,
  ActiveReservationsCount: 18,
  TotalWorkspacesCount: 7,
  PopularWorkspaces: ['Oficina Palermo Norte', 'Sala Ejecutiva Centro', 'Coworking Belgrano'],
  RecentActivity: [
    {
      Id: 'mock-1',
      WorkspaceName: 'Oficina Palermo Norte',
      CreatedAt: '2026-06-18T13:30:00-03:00',
      Status: 'Confirmed',
    },
    {
      Id: 'mock-2',
      WorkspaceName: 'Sala Ejecutiva Centro',
      CreatedAt: '2026-06-18T10:05:00-03:00',
      Status: 'Pending',
    },
    {
      Id: 'mock-3',
      WorkspaceName: 'Coworking Belgrano',
      CreatedAt: '2026-06-17T18:45:00-03:00',
      Status: 'Completed',
    },
    {
      Id: 'mock-4',
      WorkspaceName: 'Oficina Puerto Madero',
      CreatedAt: '2026-06-17T15:20:00-03:00',
      Status: 'Cancelled',
    },
    {
      Id: 'mock-5',
      WorkspaceName: 'Sala Creativa Microcentro',
      CreatedAt: '2026-06-16T09:10:00-03:00',
      Status: 'Confirmed',
    },
  ],
  DailyReservationsChart: [
    { Date: '2026-05-20', Count: 0 },
    { Date: '2026-05-21', Count: 2 },
    { Date: '2026-05-22', Count: 4 },
    { Date: '2026-05-23', Count: 1 },
    { Date: '2026-05-24', Count: 0 },
    { Date: '2026-05-25', Count: 5 },
    { Date: '2026-05-26', Count: 3 },
    { Date: '2026-05-27', Count: 6 },
    { Date: '2026-05-28', Count: 2 },
    { Date: '2026-05-29', Count: 4 },
    { Date: '2026-05-30', Count: 7 },
    { Date: '2026-05-31', Count: 0 },
    { Date: '2026-06-01', Count: 3 },
    { Date: '2026-06-02', Count: 8 },
    { Date: '2026-06-03', Count: 6 },
    { Date: '2026-06-04', Count: 9 },
    { Date: '2026-06-05', Count: 4 },
    { Date: '2026-06-06', Count: 2 },
    { Date: '2026-06-07', Count: 0 },
    { Date: '2026-06-08', Count: 5 },
    { Date: '2026-06-09', Count: 7 },
    { Date: '2026-06-10', Count: 10 },
    { Date: '2026-06-11', Count: 6 },
    { Date: '2026-06-12', Count: 8 },
    { Date: '2026-06-13', Count: 3 },
    { Date: '2026-06-14', Count: 1 },
    { Date: '2026-06-15', Count: 6 },
    { Date: '2026-06-16', Count: 9 },
    { Date: '2026-06-17', Count: 7 },
    { Date: '2026-06-18', Count: 5 },
  ],
  RevenueByWorkspaceChart: [
    { WorkspaceName: 'Palermo Norte', TotalRevenue: 48750 },
    { WorkspaceName: 'Ejecutiva Centro', TotalRevenue: 36200.5 },
    { WorkspaceName: 'Belgrano Flex', TotalRevenue: 28150 },
    { WorkspaceName: 'Puerto Madero', TotalRevenue: 21400 },
    { WorkspaceName: 'Microcentro', TotalRevenue: 14250 },
  ],
};

const chartInitialDimension = { width: 320, height: 320 };

function formatMoney(value) {
  const amount = Number(value ?? 0);
  return `$ ${moneyFormatter.format(Number.isFinite(amount) ? amount : 0)}`;
}

function formatCompactCurrency(value) {
  const amount = Number(value ?? 0);
  if (!Number.isFinite(amount)) return '$0';
  if (Math.abs(amount) >= 1000) return `$${Math.round(amount / 1000)}k`;
  return `$${amount}`;
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

function MetricCard({
  icon: Icon,
  label,
  value,
  hint,
  children,
}) {
  return (
    <article className="analytics-card">
      <div className="analytics-card__header">
        <span className="analytics-card__icon" aria-hidden="true">
          <Icon size={18} strokeWidth={2} />
        </span>
        <p>{label}</p>
      </div>
      {children ?? (
        <div className="analytics-card__body">
          <strong>{value}</strong>
          {hint && <span>{hint}</span>}
        </div>
      )}
    </article>
  );
}

function PopularWorkspacesList({ workspaces }) {
  if (workspaces.length === 0) {
    return (
      <div className="analytics-card__body">
        <strong>Sin datos</strong>
        <span>No hay reservas registradas</span>
      </div>
    );
  }

  return (
    <ol className="analytics-ranking">
      {workspaces.slice(0, 3).map((workspace, index) => (
        <li key={workspace}>
          <span>{index + 1}</span>
          <strong>{workspace}</strong>
        </li>
      ))}
    </ol>
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
    <ResponsiveContainer
      width="100%"
      height="100%"
      minWidth={1}
      minHeight={280}
      initialDimension={chartInitialDimension}
    >
      <AreaChart data={data} margin={{ top: 8, right: 14, bottom: 0, left: -10 }}>
        <defs>
          <linearGradient id="reservationsGradient" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#2563eb" stopOpacity={0.15} />
            <stop offset="95%" stopColor="#2563eb" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid stroke="#e6ebf2" strokeDasharray="3 3" vertical={false} />
        <XAxis
          dataKey="date"
          tickFormatter={formatShortDate}
          tickLine={false}
          axisLine={false}
          minTickGap={18}
          tick={{ fill: '#6b7280', fontSize: 12 }}
        />
        <YAxis
          allowDecimals={false}
          domain={[0, 'auto']}
          tickLine={false}
          axisLine={false}
          width={34}
          tick={{ fill: '#6b7280', fontSize: 12 }}
        />
        <Tooltip
          formatter={(value) => [Number(value).toLocaleString('es-AR'), 'Reservas']}
          labelFormatter={(label) => `Fecha: ${formatShortDate(label)}`}
        />
        <Area
          type="monotone"
          dataKey="count"
          stroke="#2563eb"
          strokeWidth={2.5}
          fill="url(#reservationsGradient)"
          dot={{ r: 2.5, strokeWidth: 2, fill: '#2563eb' }}
          activeDot={{ r: 4 }}
          name="Reservas"
          connectNulls
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function RevenueBarChart({ data }) {
  if (!hasPositiveValue(data, 'totalRevenue')) return <ChartEmptyState />;

  return (
    <ResponsiveContainer
      width="100%"
      height="100%"
      minWidth={1}
      minHeight={280}
      initialDimension={chartInitialDimension}
    >
      <BarChart
        data={data}
        layout="vertical"
        margin={{ top: 8, right: 18, bottom: 0, left: 12 }}
        barCategoryGap={14}
      >
        <CartesianGrid stroke="#e6ebf2" strokeDasharray="3 3" horizontal={false} />
        <XAxis
          type="number"
          tickLine={false}
          axisLine={false}
          tickFormatter={formatCompactCurrency}
          tick={{ fill: '#6b7280', fontSize: 12 }}
        />
        <YAxis
          dataKey="workspaceName"
          type="category"
          tickLine={false}
          axisLine={false}
          width={124}
          tick={{ fill: '#374151', fontSize: 12 }}
        />
        <Tooltip
          formatter={(value) => [formatMoney(value), 'Ingresos']}
          labelFormatter={(label) => `Oficina: ${label}`}
        />
        <Bar dataKey="totalRevenue" fill="#7c3aed" radius={[0, 6, 6, 0]} name="Ingresos" />
      </BarChart>
    </ResponsiveContainer>
  );
}

function AnalyticsDashboard() {
  const { data, isLoading, isError } = useHostDashboard({ enabled: !USE_MOCK_DASHBOARD });
  const dashboardData = USE_MOCK_DASHBOARD ? MOCK_HOST_DASHBOARD : data;
  const dashboard = useMemo(() => normalizeDashboard(dashboardData), [dashboardData]);

  if (!USE_MOCK_DASHBOARD && isLoading) return <DashboardSkeleton />;

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
        <span className="analytics-dashboard__period">
          <Calendar size={13} aria-hidden="true" />
          Últimos 30 días
        </span>
      </header>

      {!USE_MOCK_DASHBOARD && isError && (
        <div className="analytics-alert" role="alert">
          No se pudo cargar la información del dashboard.
        </div>
      )}

      <section className="analytics-dashboard__metrics" aria-label="Resumen de rendimiento">
        <MetricCard
          icon={DollarSign}
          label="Ingresos Totales"
          value={formatMoney(dashboard.totalRevenue)}
          hint="Reservas válidas"
        />
        <MetricCard
          icon={CalendarCheck}
          label="Reservas Activas"
          value={dashboard.activeReservationsCount}
          hint="Confirmadas vigentes"
        />
        <MetricCard
          icon={Building2}
          label="Oficinas Totales"
          value={dashboard.totalWorkspacesCount}
          hint="Espacios publicados"
        />
        <MetricCard icon={Star} label="Oficinas Populares">
          <PopularWorkspacesList workspaces={dashboard.popularWorkspaces} />
        </MetricCard>
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
