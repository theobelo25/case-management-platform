import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AboutComponent } from '@app/features/landing/components/about/about.component';
import { FeaturesComponent } from '@app/features/landing/components/features/features.component';
import { HeroComponent } from '@app/features/landing/components/hero/hero.component';
import { HowItWorksComponent } from '@app/features/landing/components/how-it-works/how-it-works.component';

@Component({
  selector: 'app-landing',
  imports: [HeroComponent, FeaturesComponent, HowItWorksComponent, AboutComponent],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}

