import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-demo',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './demo.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DemoComponent {
  protected loomEmbedUrl: string | null = null;
}
