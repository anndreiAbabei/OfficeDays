<script lang="ts">
  import { configuration } from '$lib/features/privacy/configuration';
</script>

<svelte:head><title>Privacy notice · Office Days</title></svelte:head>

<article class="card">
  <h1>Privacy notice</h1>
  {#if $configuration.status === 'ready'}
    {#if $configuration.privacyNotice}
      {@const notice = $configuration.privacyNotice}
      <h2>Who handles your data?</h2>
      <p>{notice.operatorName} operates this instance and is responsible for its handling of personal data.
        Other users and employers cannot look up your records in Office Days. The operator has technical access
        to the database to run and support the service.</p>

      <h2>Using an account</h2>
      <p>Your username and password let you sign in. The server stores a password hash. Your holiday calendar,
        timezone, attendance requirements, visits, plans and vacations allow Office Days to calculate your
        working days and reports. The server also saves your preferences and, if you use an integration,
        API token names, identifiers and usage information.</p>
      <p>You can register without an email address. If you add one, it is stored in your profile.
        Password recovery by email is not available in this version.</p>

      <h2>Using guest mode</h2>
      <p>Your guest profile and attendance history stay in this browser. It contacts the server to load
        Office Days and download holiday calendars. Clearing site data removes guest history.
        To keep a copy, download it from your guest profile first.</p>

      <h2>How long records are kept</h2>
      <p>Your account and history remain available until you delete the account. Inactivity alone
        does not close it. Deletion removes your records from the live database. Existing backups
        and operational logs expire according to the operator’s retention settings.</p>

      <h2>Hosting, purposes and legal bases</h2>
      <p class="operator-notice">{notice.details}</p>

      <h2>Your rights and contact</h2>
      <p>In Profile you can download your account data, update your settings or delete your account.
        To ask for access, correction, erasure, restriction or portability, or to object to processing
        based on legitimate interests, contact the operator. These rights depend on the circumstances.
        You can also complain to a data protection authority.</p>
      
        <p>Privacy contact: <a href={'mailto:' + notice.contactEmail}>{notice.contactEmail}</a></p>
    {:else}
      <p>This instance has no published privacy notice.</p>
    {/if}
  {:else if $configuration.status === 'loading'}
    <p role="status">Loading the privacy notice…</p>
  {/if}
  <a href="/">Return to Office Days</a>
</article>

<style>
  article { 
    max-width: 70ch; 
    margin: 0 auto; 
  }
  
  h2 { 
    margin: 1.4rem 0 .45rem; 
  }
  
  .operator-notice { 
    white-space: pre-wrap; 
    overflow-wrap: anywhere; 
  }
</style>
