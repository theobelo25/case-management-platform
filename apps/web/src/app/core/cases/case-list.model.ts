import { CaseItem } from '@app/features/cases/models/cases.types';

export interface CaseListResult {
  items: CaseItem[];
  nextCursor: string | null;
  previousCursor: string | null;
}
