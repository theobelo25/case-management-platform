import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import type { AuthResponseDto } from '@app/core/auth/auth-api.service';
import { AuthService } from '@app/core/auth/auth.service';
import { EMPTY } from 'rxjs';
import { ProtectedHeaderComponent } from './protected-header.component';

describe('ProtectedHeaderComponent', () => {
  let fixture: ComponentFixture<ProtectedHeaderComponent>;
  let signOutMock: ReturnType<typeof vi.fn>;

  const sessionDto: AuthResponseDto = {
    accessToken: 't',
    expiresAtUtc: '2026-01-01T00:00:00Z',
    userId: '11111111-1111-1111-1111-111111111111',
    email: 'user@test.com',
    fullName: 'Test User',
  };

  beforeEach(async () => {
    signOutMock = vi.fn().mockReturnValue(EMPTY);

    await TestBed.configureTestingModule({
      imports: [ProtectedHeaderComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            session: signal(sessionDto).asReadonly(),
            signOut: signOutMock,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProtectedHeaderComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('calls signOut and navigates to sign-in when Log out is chosen', () => {
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl');

    const userButton = fixture.nativeElement.querySelector(
      '[aria-controls="protected-header-user-menu"]',
    ) as HTMLButtonElement;
    userButton.click();
    fixture.detectChanges();

    const menuItems = fixture.nativeElement.querySelectorAll('[role="menuitem"]');
    const logoutEl = Array.from(menuItems as NodeListOf<HTMLElement>).find(
      (el) => el.textContent?.trim() === 'Log out',
    );
    expect(logoutEl).toBeTruthy();
    logoutEl!.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    expect(signOutMock).toHaveBeenCalledTimes(1);
    expect(navigateByUrl).toHaveBeenCalledWith('/auth/sign-in');
  });
});
