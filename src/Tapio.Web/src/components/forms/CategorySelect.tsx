import { BUSINESS_CATEGORIES } from '../../data/businessCategories';

type Props = {
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  id?: string;
};

export function CategorySelect({ value, onChange, required, id }: Props) {
  const inList = !value || BUSINESS_CATEGORIES.includes(value as (typeof BUSINESS_CATEGORIES)[number]);
  const selectValue = inList ? value : 'Diğer';

  return (
    <div className="form-group">
      <label htmlFor={id}>Kategori</label>
      <select
        id={id}
        className="select"
        value={selectValue}
        onChange={e => onChange(e.target.value === 'Diğer' ? '' : e.target.value)}
        required={required && selectValue !== 'Diğer'}
      >
        <option value="">Kategori seçin</option>
        {BUSINESS_CATEGORIES.map(c => (
          <option key={c} value={c}>{c}</option>
        ))}
      </select>
      {selectValue === 'Diğer' && (
        <input
          className="input"
          style={{ marginTop: '0.5rem' }}
          placeholder="Kategori adını yazın"
          value={value}
          onChange={e => onChange(e.target.value)}
          required={required}
        />
      )}
    </div>
  );
}
