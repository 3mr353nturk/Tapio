import { useId, useMemo } from 'react';
import { getDistrictsForProvince, isKnownProvince, normalizeProvince, TURKEY_PROVINCES } from '../../data/turkiyeLocations';

type Props = {
  city: string;
  district: string;
  onCityChange: (city: string) => void;
  onDistrictChange: (district: string) => void;
  required?: boolean;
  className?: string;
};

export function LocationFields({ city, district, onCityChange, onDistrictChange, required, className }: Props) {
  const provinceListId = useId();
  const districtListId = useId();
  const normalizedCity = normalizeProvince(city);
  const districts = useMemo(() => getDistrictsForProvince(normalizedCity), [normalizedCity]);
  const districtOptions = useMemo(() => {
    if (!district) return districts;
    if (districts.includes(district)) return districts;
    return [district, ...districts];
  }, [district, districts]);

  const handleProvincePick = (value: string) => {
    const next = normalizeProvince(value);
    onCityChange(next);
    if (next !== normalizeProvince(city)) {
      onDistrictChange('');
    }
  };

  return (
    <div className={className ?? 'location-fields'}>
      <div className="form-group">
        <label htmlFor={`${provinceListId}-input`}>İl</label>
        <input
          id={`${provinceListId}-input`}
          className="input"
          list={provinceListId}
          value={city}
          onChange={e => handleProvincePick(e.target.value)}
          onBlur={e => handleProvincePick(e.target.value)}
          placeholder="İl seçin veya yazın"
          required={required}
          autoComplete="address-level1"
        />
        <datalist id={provinceListId}>
          {TURKEY_PROVINCES.map(p => (
            <option key={p} value={p} />
          ))}
        </datalist>
        {city && !isKnownProvince(city) && (
          <p className="error-text" style={{ marginTop: '0.35rem' }}>Listeden bir il seçin (81 il).</p>
        )}
      </div>
      <div className="form-group">
        <label htmlFor={`${districtListId}-input`}>İlçe</label>
        <input
          id={`${districtListId}-input`}
          className="input"
          list={districtOptions.length > 0 ? districtListId : undefined}
          value={district}
          onChange={e => onDistrictChange(e.target.value)}
          placeholder={normalizedCity ? 'İlçe seçin veya yazın' : 'Önce il seçin'}
          required={required}
          disabled={!normalizedCity}
          autoComplete="address-level2"
        />
        {districtOptions.length > 0 && (
          <datalist id={districtListId}>
            {districtOptions.map(d => (
              <option key={d} value={d} />
            ))}
          </datalist>
        )}
      </div>
    </div>
  );
}
