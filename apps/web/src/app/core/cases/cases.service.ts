import { inject, Injectable } from '@angular/core';
import { addCaseRequestDto, CaseListQueryParams, CasesApiService } from './cases-api.service';
import { map, Observable } from 'rxjs';
import { toCaseItem, toCaseItems } from './case-item.mapper';
import { CaseListResult } from './case-list.model';
import { CaseItem } from '@app/features/cases/models/cases.types';

@Injectable({ providedIn: 'root' })
export class CasesService {
  private readonly api = inject(CasesApiService);

  listCases(params?: CaseListQueryParams): Observable<CaseListResult> {
    return this.api.getCases(params).pipe(
      map((response) => ({
        items: toCaseItems(response.items),
        nextCursor: response.nextCursor,
        previousCursor: response.previousCursor,
      })),
    );
  }

  addCase(body: addCaseRequestDto): Observable<CaseItem> {
    return this.api.addCase(body).pipe(map(toCaseItem));
  }

  updateCase(body: addCaseRequestDto): Observable<CaseItem> {
    return this.api.updateCase(body).pipe(map(toCaseItem));
  }
}
