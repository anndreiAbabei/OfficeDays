<script lang="ts">
  import { formatLifetime } from '$lib/features/privacy/formatLifetime';
  let { persistentLoginSeconds }: { persistentLoginSeconds: number } = $props();
  const panelId = $props.id();
  let closeButton: HTMLButtonElement;

  function focusPanel(event: ToggleEvent): void {
    if (event.newState === 'open') 
      closeButton.focus({ preventScroll: true });
  }
</script>

<button class="notice-trigger" type="button" popovertarget={panelId} aria-haspopup="dialog">
  Cookies and browser storage
</button>

<div id={panelId} popover="auto" role="dialog" aria-labelledby={panelId + '-heading'} ontoggle={focusPanel}>
  <header>
    <h2 id={panelId + '-heading'}>Cookies used by Office Days</h2>
    <button bind:this={closeButton} type="button" class="close" aria-label="Close cookie information"
      popovertarget={panelId} popovertargetaction="hide"><span aria-hidden="true">×</span></button>
  </header>
  <div class="content">
    <dl>
      <dt>Sign-in · <code>OfficeDays.Session</code></dt>
      <dd>Keeps you signed in for the browser session. If you select “Keep me signed in on this device”,
        the cookie lasts up to {formatLifetime(persistentLoginSeconds)} and can be renewed while you use the app.
        Signing out removes it.</dd>

      <dt>Request protection · <code>OfficeDays.Antiforgery</code></dt>
      <dd>Helps prevent other sites from submitting forms or changing your data on your behalf.
        This cookie lasts for the browser session.</dd>

      <dt>Account theme · <code>appearance</code></dt>
      <dd>Remembers your Light or Dark choice so pages open in that theme. It lasts up to a year
        from its last update. To remove it, select System under Profile → Appearance and save.</dd>
    </dl>

    <h2>Other browser storage</h2>
    <p>In guest mode, this browser keeps your attendance, plans, vacations, preferences and holiday
      calendars in local storage. Clearing site data erases your guest history. To keep a copy,
      choose <strong>Download guest data</strong> in your guest profile first.</p>
    <p>For offline access, Office Days caches public app files such as pages, scripts and icons.
      This cache excludes account data and API responses.</p>
    <p>Office Days includes no advertising or analytics trackers.</p>
  </div>
</div>

<style>
  .notice-trigger {
    display: block;
    margin: 0 auto;
    padding: .4rem .7rem;
    background: transparent;
    color: var(--green-dark);
    font: inherit;
    text-decoration: underline;
    text-underline-offset: .16em;
  }

  [popover] {
    --gap: .8rem;
    position: fixed;
    inset: auto max(var(--gap), env(safe-area-inset-right)) max(var(--gap), env(safe-area-inset-bottom)) auto;
    width: min(28rem, calc(100% - max(var(--gap), env(safe-area-inset-left)) - max(var(--gap), env(safe-area-inset-right))));
    max-height: min(38rem, 75dvh);
    margin: 0;
    padding: 0;
    border: 1px solid var(--line);
    border-radius: .85rem;
    background: var(--paper);
    color: var(--ink);
    box-shadow: 0 .6rem 2.5rem rgb(0 0 0 / 18%);
    text-align: start;
    font-size: .95rem;
    overflow: auto;
    overscroll-behavior: contain;
  }

  header {
    position: sticky;
    top: 0; 
    background: var(--paper); 
    display: flex; 
    align-items: center; 
    gap: .5rem; 
    justify-content: space-between; 
    padding: .4rem .5rem .4rem 1rem; 
    border-bottom: 1px solid var(--line); 
  }
  
  header h2 { 
    margin: 0; 
  }
  
  .close { 
    flex: none; 
    width: 44px; 
    padding: 0;
    background: transparent; 
    color: var(--muted); 
    font-size: 1.5rem; 
  }
  
  .close:hover { 
    background: var(--secondary); 
    color: var(--ink); 
  }
  
  .content { 
    padding: 1rem; 
    overflow-wrap: anywhere; 
  }
  
  dl { 
    margin: 0; 
  }
  
  dt { 
    font-weight: 650; 
  }
  
  dd { 
    margin: .2rem 0 1rem; 
  }
  
  p { 
    margin: .5rem 0 0; 
  }
</style>
