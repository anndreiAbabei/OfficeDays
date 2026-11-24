export const platforms = {
  ios: 'iOS/iPadOS',
  android: 'Android',
  macos: 'macOS',
  windows: 'Windows',
  linux: 'Linux'
} as const;

export type Platform = keyof typeof platforms;

export const groups: { name: string; devices: Platform[] }[] = [
  { name: 'Mobile', devices: ['ios', 'android'] },
  { name: 'Desktop', devices: ['macos', 'windows', 'linux'] }
];

export function detectPlatform({ userAgent, maxTouchPoints }: Pick<Navigator, 'userAgent' | 'maxTouchPoints'>): Platform | null {
  if (/iPhone|iPod|iPad/i.test(userAgent) || (/Macintosh/i.test(userAgent) && maxTouchPoints > 1)) return 'ios';
  if (/Android/i.test(userAgent)) return 'android';
  if (/Macintosh|Mac OS X/i.test(userAgent)) return 'macos';
  if (/Windows/i.test(userAgent)) return 'windows';
  if (/Linux/i.test(userAgent)) return 'linux';
  return null;
}
