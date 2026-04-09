import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-add-case-button',
  standalone: true,
  templateUrl: './add-case-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddCaseButton {}
