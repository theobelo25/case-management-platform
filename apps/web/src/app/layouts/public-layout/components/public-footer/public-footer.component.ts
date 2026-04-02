import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-public-footer',
  standalone: true,
  templateUrl: './public-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicFooterComponent {}
