import { AgePipe } from './age-pipe';

describe('AgePipe', () => {
  let pipe: AgePipe;

  beforeEach(() => {
    pipe = new AgePipe();
    // Freeze "today" for deterministic age calculations
    spyOn(Date, 'now').and.returnValue(new Date(2025, 0, 2).valueOf());
  });

  it('should calculate age correctly when birthday has passed this year', () => {
    const dob = new Date(1990, 0, 1).toISOString(); 
    const today = new Date();

    const expected = today.getFullYear() - 1990;

    expect(pipe.transform(dob)).toBe(expected);
  });

  it('should calculate age correctly when birthday has NOT passed this year', () => {
    const today = new Date();
    const dob = new Date(
      2000,
      today.getMonth() + 1,          
      today.getDate()
    ).toISOString();

    const expected = today.getFullYear() - 2000 - 1;

    expect(pipe.transform(dob)).toBe(expected);
  });

  it('should return NaN for invalid dates', () => {
    expect(pipe.transform('not-a-date')).toBeNaN();
  });
});
