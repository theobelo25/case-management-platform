export interface OrganizationDetailViewModel {
  id: string;
  name: string;
  createdAtUtc: string;
  isArchived: boolean;
}

export interface OrganizationMemberViewModel {
  id: string;
  name: string;
  email?: string;
  role: string;
  joinedAtUtc?: string;
}
