type GuideStep = { title: string; instruction: string; image?: string; alt?: string };

export const iosSteps: GuideStep[] = [
  { title: 'Open the iCloud link', instruction: 'Choose Add to Shortcuts on this page. In the iCloud preview, tap Get Shortcut to open the Shortcuts app.', image: 'step-1', alt: 'iCloud preview with Get Shortcut highlighted' },
  { title: 'Start the import', instruction: 'Tap Set Up Shortcut to personalize your copy.', image: 'step-2', alt: 'Set Up Shortcut button in the Shortcuts preview' },
  { title: 'Choose your office', instruction: 'Tap Choose next to Location to open the location picker.', image: 'step-3', alt: 'Office location question with the Choose control highlighted' },
  { title: 'Confirm the location', instruction: 'Search for your office, select the matching address, then tap the checkmark. Apple Park is an example.', image: 'step-4', alt: 'Location picker with an example office and confirmation checkmark' },
  { title: 'Continue with this location', instruction: 'Check the selected office and tap Next.', image: 'step-5', alt: 'Selected office location above the Next button' },
  { title: 'Check your OfficeDays address', instruction: 'The server address must match the one on this page. Keep the default if it matches, or paste your address. If your template skips this question, check the URL in the shortcut editor before running or enabling it.' },
  { title: 'Paste the Authorization header', instruction: 'Copy the complete Authorization header from OfficeDays, including Bearer. Paste it into the question, then tap Add Shortcut.', image: 'step-6', alt: 'Authorization question and Add Shortcut button, with the credential hidden' },
  { title: 'Find the installed shortcut', instruction: 'Your shortcut now appears in All Shortcuts. Use the back arrow to open the library.', image: 'step-7', alt: 'Installed OfficeDays shortcut in All Shortcuts with the back arrow highlighted' },
  { title: 'Open Automation', instruction: 'In the Shortcuts library, tap Automation to manage your arrival trigger.', image: 'step-8', alt: 'Automation row highlighted in the Shortcuts library' },
  { title: 'Enable the arrival automation', instruction: 'Turn on the entry for your office. If there is no entry, follow the arrival setup instructions below.', image: 'step-9', alt: 'Enabled switch beside the personal office arrival automation' }
];

export function screenshotUrl(image: string): string {
  return '/automation/ios/' + image + '.webp';
}
