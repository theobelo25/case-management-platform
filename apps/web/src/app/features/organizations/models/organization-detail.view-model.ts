/** Client-only view models for the organization detail page (API wiring comes later). */
export interface OrganizationDetailViewModel {
  id: string;
  name: string;
  createdAtUtc: string;
}

export interface OrganizationMemberViewModel {
  id: string;
  name: string;
  email?: string;
  role: string;
  joinedAtUtc?: string;
}
