import type { OnboardingResponse } from '../../types/api.types';

export function OnboardingChecklist({ data }: { data: OnboardingResponse }) {
  if (data.completedCount === data.totalCount) return null;

  return (
    <div className="card" style={{ marginBottom: '1rem' }}>
      <h3 style={{ marginTop: 0 }}>İşletmenizi tamamlayalım</h3>
      <p style={{ color: 'var(--color-muted)' }}>{data.completedCount}/{data.totalCount} tamamlandı</p>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {data.steps.map(step => (
          <li key={step.key} style={{ padding: '0.35rem 0' }}>
            {step.completed ? '✓' : '○'} {step.label}
          </li>
        ))}
      </ul>
    </div>
  );
}
