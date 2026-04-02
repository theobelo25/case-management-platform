import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FeaturesComponent } from '../../components/features/features.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { HowItWorksComponent } from '../../components/how-it-works/how-it-works.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [HeroComponent, FeaturesComponent, HowItWorksComponent],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
