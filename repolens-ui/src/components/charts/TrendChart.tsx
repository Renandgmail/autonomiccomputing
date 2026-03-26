import React from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ChartOptions
} from 'chart.js';
import { Line } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface TrendDataPoint {
  date: string;
  value: number;
}

interface TrendChartProps {
  data: TrendDataPoint[];
  title: string;
  color?: string;
  yAxisLabel?: string;
  height?: number;
}

const TrendChart: React.FC<TrendChartProps> = ({
  data,
  title,
  color = '#1976d2',
  yAxisLabel = 'Value',
  height = 300
}) => {
  const chartData = {
    labels: data.map(d => new Date(d.date).toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric' 
    })),
    datasets: [
      {
        label: title,
        data: data.map(d => d.value),
        borderColor: color,
        backgroundColor: color + '20',
        borderWidth: 2,
        fill: true,
        tension: 0.3,
        pointBackgroundColor: color,
        pointBorderColor: color,
        pointRadius: 4,
        pointHoverRadius: 6,
      },
    ],
  };

  const options: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
        display: false, // Hide legend since we have the title
      },
      title: {
        display: true,
        text: title,
        font: {
          size: 16,
          weight: 'bold',
        },
        padding: {
          bottom: 20,
        },
      },
      tooltip: {
        mode: 'index',
        intersect: false,
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleFont: {
          size: 14,
        },
        bodyFont: {
          size: 13,
        },
        padding: 12,
        cornerRadius: 8,
        callbacks: {
          title: (context) => {
            const dataIndex = context[0]?.dataIndex;
            if (dataIndex !== undefined && data[dataIndex]) {
              return new Date(data[dataIndex].date).toLocaleDateString('en-US', {
                weekday: 'short',
                year: 'numeric',
                month: 'short',
                day: 'numeric'
              });
            }
            return '';
          },
          label: (context) => {
            const value = context.parsed.y;
            if (value === null || value === undefined) return '';
            if (title.toLowerCase().includes('size') || title.toLowerCase().includes('bytes')) {
              return `${yAxisLabel}: ${formatBytes(value)}`;
            } else if (title.toLowerCase().includes('score') || title.toLowerCase().includes('percentage')) {
              return `${yAxisLabel}: ${value.toFixed(1)}%`;
            } else {
              return `${yAxisLabel}: ${value.toLocaleString()}`;
            }
          },
        },
      },
    },
    interaction: {
      mode: 'nearest',
      axis: 'x',
      intersect: false,
    },
    scales: {
      x: {
        display: true,
        title: {
          display: true,
          text: 'Date',
          font: {
            size: 12,
            weight: 'bold',
          },
        },
        grid: {
          display: false,
        },
        ticks: {
          maxTicksLimit: 8,
          font: {
            size: 11,
          },
        },
      },
      y: {
        display: true,
        title: {
          display: true,
          text: yAxisLabel,
          font: {
            size: 12,
            weight: 'bold',
          },
        },
        beginAtZero: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.1)',
        },
        ticks: {
          font: {
            size: 11,
          },
          callback: function(value) {
            if (value === null || value === undefined) return '';
            const numValue = Number(value);
            if (title.toLowerCase().includes('size') || title.toLowerCase().includes('bytes')) {
              return formatBytes(numValue);
            } else if (title.toLowerCase().includes('score') || title.toLowerCase().includes('percentage')) {
              return numValue.toFixed(0) + '%';
            } else if (numValue >= 1000) {
              return (numValue / 1000).toFixed(1) + 'K';
            } else {
              return numValue.toString();
            }
          },
        },
      },
    },
  };

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  if (!data || data.length === 0) {
    return (
      <div 
        style={{ 
          height: height,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f5f5f5',
          border: '1px solid #e0e0e0',
          borderRadius: '8px',
          color: '#666'
        }}
      >
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '16px', fontWeight: 'bold', marginBottom: '8px' }}>
            {title}
          </div>
          <div style={{ fontSize: '14px' }}>
            No data available
          </div>
        </div>
      </div>
    );
  }

  return (
    <div style={{ height: height, width: '100%' }}>
      <Line data={chartData} options={options} />
    </div>
  );
};

export default TrendChart;
