import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrganizationsService } from '@app/core/organizations/organizations.service';
import { PaginationControlsComponent } from '@app/shared/components/pagination-controls/pagination-controls.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-organizations-list',
  standalone: true,
  imports: [RouterLink, PaginationControlsComponent],
  templateUrl: './organizations-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationsListComponent implements OnInit {
  protected readonly organizations = inject(OrganizationsService);
  protected readonly loading = signal(true);
  protected readonly pageSize = signal(10);
  protected readonly skip = signal(0);

  ngOnInit(): void {
    this.loadPage();
  }

  protected onSkipChange(nextSkip: number): void {
    this.skip.set(nextSkip);
    this.loadPage();
  }

  protected onLimitChange(nextLimit: number): void {
    this.pageSize.set(nextLimit);
    this.skip.set(0);
    this.loadPage();
  }

  private loadPage(): void {
    this.loading.set(true);
    this.organizations
      .listOrganizationsForUser(this.skip(), this.pageSize())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        error: () => {
          /* LoadError on service */
        },
      });
  }
}
