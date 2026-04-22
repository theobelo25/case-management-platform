import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrganizationsService } from '@app/core/organizations/organizations.service';
import { PaginationControlsComponent } from '@app/shared/components/pagination-controls/pagination-controls.component';
import { SectionCardComponent } from '@app/shared/components/section-card/section-card.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-organizations-list',
  imports: [RouterLink, PaginationControlsComponent, SectionCardComponent],
  templateUrl: './organizations-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationsListComponent {
  protected readonly organizations = inject(OrganizationsService);
  protected readonly loading = signal(true);
  protected readonly pageSize = signal(10);
  protected readonly skip = signal(0);

  constructor() {
    effect((onCleanup) => {
      const skip = this.skip();
      const limit = this.pageSize();

      this.organizations.clearLoadError();
      this.loading.set(true);

      const sub = this.organizations
        .listOrganizationsForUser(skip, limit)
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe({
          error: () => {
            /* LoadError on service */
          },
        });

      onCleanup(() => sub.unsubscribe());
    });
  }

  protected onSkipChange(nextSkip: number): void {
    this.skip.set(nextSkip);
  }

  protected onLimitChange(nextLimit: number): void {
    this.pageSize.set(nextLimit);
    this.skip.set(0);
  }
}

