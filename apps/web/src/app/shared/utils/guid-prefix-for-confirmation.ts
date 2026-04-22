/** Length of the GUID prefix the user must retype to confirm destructive actions. */
export const GUID_CONFIRMATION_PREFIX_LENGTH = 6;

export function guidPrefixForConfirmation(id: string): string {
  const t = id.trim();
  return t.slice(0, GUID_CONFIRMATION_PREFIX_LENGTH).toLowerCase();
}
