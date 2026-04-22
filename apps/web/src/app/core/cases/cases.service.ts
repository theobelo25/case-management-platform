import { inject, Injectable } from '@angular/core';
import {
  addCaseCommentRequestDto,
  addCaseRequestDto,
  assignCaseRequestDto,
  BulkCasesRequestDto,
  BulkCasesResultDto,
  CaseListQueryParams,
  CaseStatusCountsDto,
  CaseVolumeOverTimeDto,
  FirstResponseTimeOverTimeDto,
  CasesApiService,
  updateCaseMetadataRequestDto,
} from './cases-api.service';
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

  getCase(id: string): Observable<CaseItem> {
    return this.api.getCase(id).pipe(map(toCaseItem));
  }

  updateCase(body: addCaseRequestDto): Observable<CaseItem> {
    return this.api.updateCase(body).pipe(map(toCaseItem));
  }

  updateCaseMetadata(body: updateCaseMetadataRequestDto): Observable<CaseItem> {
    return this.api.updateCaseMetadata(body).pipe(map(toCaseItem));
  }

  addCaseComment(body: addCaseCommentRequestDto): Observable<CaseItem> {
    return this.api.addCaseComment(body).pipe(map(toCaseItem));
  }

  assignCase(body: assignCaseRequestDto): Observable<CaseItem> {
    return this.api.assignCase(body).pipe(map(toCaseItem));
  }

  deleteCase(caseId: string): Observable<void> {
    return this.api.deleteCase(caseId);
  }

  archiveCase(caseId: string): Observable<CaseItem> {
    return this.api.archiveCase(caseId).pipe(map(toCaseItem));
  }

  unarchiveCase(caseId: string): Observable<CaseItem> {
    return this.api.unarchiveCase(caseId).pipe(map(toCaseItem));
  }

  bulkCases(body: BulkCasesRequestDto): Observable<BulkCasesResultDto> {
    return this.api.bulkCases(body);
  }

  getCaseVolumeOverTime(days?: number): Observable<CaseVolumeOverTimeDto> {
    return this.api.getCaseVolumeOverTime(days);
  }

  getFirstResponseTimeOverTime(days?: number): Observable<FirstResponseTimeOverTimeDto> {
    return this.api.getFirstResponseTimeOverTime(days);
  }

  getCaseStatusCounts(): Observable<CaseStatusCountsDto> {
    return this.api.getCaseStatusCounts();
  }
}
