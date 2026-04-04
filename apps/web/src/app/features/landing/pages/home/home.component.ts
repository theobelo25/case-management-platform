import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FeaturesComponent } from '@app/features/landing/components/features/features.component';
import { HeroComponent } from '@app/features/landing/components/hero/hero.component';
import { HowItWorksComponent } from '@app/features/landing/components/how-it-works/how-it-works.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [HeroComponent, FeaturesComponent, HowItWorksComponent],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
