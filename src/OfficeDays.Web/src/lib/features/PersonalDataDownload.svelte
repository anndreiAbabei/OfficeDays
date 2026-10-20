<script lang="ts">
  import { downloadPersonalData, errorMessage } from '$lib/api';
  import Feedback from '$lib/components/Feedback.svelte';

  let exporting = $state(false); 
  let exportError = $state(''); 
  let exportMessage = $state('');

  async function exportData(): Promise<void> {
    exporting = true; 
    exportError = ''; 
    exportMessage = '';
    try {
      await downloadPersonalData();
      
      exportMessage = 'Download started. Keep this file private: it contains your personal data.';
    } catch (failure) {
      exportError = `Could not download your data. ${errorMessage(failure)} Please try again.`;
    } finally {
      exporting = false;
    }
  }

</script>

<section class="card export-user">
  <h2>Download your data</h2>
  <p>Save all your profile settings, attendance requirements, attendance, plans, vacations, and holiday calendar as a JSON file.</p>
  <p class="muted">Includes the app version and calendar snapshot for future migration. Passwords and API tokens are excluded. Import is not available yet.</p>
  <button type="button" onclick={exportData} disabled={exporting}>
    {exporting ? 'Preparing download…' : 'Download my data (JSON)'}
  </button>
  <Feedback error={exportError} message={exportMessage} />
</section>

<style>
  .export-user > button {
    margin-top: 2rem;
  }
</style>
