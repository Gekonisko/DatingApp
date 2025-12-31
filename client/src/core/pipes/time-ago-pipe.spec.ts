import { TimeAgoPipe } from './time-ago-pipe';

describe('TimeAgoPipe', () => {
  it('create an instance', () => {
    const pipe = new TimeAgoPipe();
    expect(pipe).toBeTruthy();
  });

  it('returns "Just now" for recent times (<30s)', () => {
    const pipe = new TimeAgoPipe();
    const tenSecondsAgo = new Date(Date.now() - 10 * 1000).toISOString();
    expect(pipe.transform(tenSecondsAgo)).toBe('Just now');
  });

  it('returns seconds ago for <1 minute', () => {
    const pipe = new TimeAgoPipe();
    const fortyFiveSecondsAgo = new Date(Date.now() - 45 * 1000).toISOString();
    expect(pipe.transform(fortyFiveSecondsAgo)).toBe('45 seconds ago');
  });

  it('returns "1 minute ago" for ~60s', () => {
    const pipe = new TimeAgoPipe();
    const oneMinuteAgo = new Date(Date.now() - 60 * 1000).toISOString();
    expect(pipe.transform(oneMinuteAgo)).toBe('1 minute ago');
  });

  it('returns minutes/hours/days/months/years correctly', () => {
    const pipe = new TimeAgoPipe();

    const twoMinutesAgo = new Date(Date.now() - 2 * 60 * 1000).toISOString();
    expect(pipe.transform(twoMinutesAgo)).toBe('2 minutes ago');

    const threeHoursAgo = new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString();
    expect(pipe.transform(threeHoursAgo)).toBe('3 hours ago');

    const fiveDaysAgo = new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString();
    expect(pipe.transform(fiveDaysAgo)).toBe('5 days ago');

    const twoMonthsAgo = new Date(Date.now() - 60 * 24 * 60 * 60 * 1000).toISOString();
    // 60 days ~ 2 months (approx using 30-day month in pipe)
    expect(pipe.transform(twoMonthsAgo)).toBe('2 months ago');

    const twoYearsAgo = new Date(Date.now() - 2 * 365 * 24 * 60 * 60 * 1000).toISOString();
    expect(pipe.transform(twoYearsAgo)).toBe('2 years ago');
  });

  it('returns original value when input is empty or invalid', () => {
    const pipe = new TimeAgoPipe();
    expect(pipe.transform('')).toBe('');
    const invalid = 'not-a-date';
    expect(pipe.transform(invalid)).toBe(invalid);
  });
});
