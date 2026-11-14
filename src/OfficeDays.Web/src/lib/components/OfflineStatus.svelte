<script lang="ts">
  import { onMount } from 'svelte';
  import { dev } from '$app/environment';

  type Status = 'hidden' | 'preparing' | 'ready' | 'failed';
  let status = $state<Status>('hidden');
  let offline = $state(false);

  async function prepare(): Promise<void> {
    status = 'preparing';
    
    try {
      const registration = await navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' });
      const worker = registration.active ?? registration.installing ?? registration.waiting;
      
      if (!worker) 
        throw new Error('Worker unavailable');
      
      if (worker.state !== 'activated') {
        await new Promise<void>((resolve, reject): void => {
          const changed = (): void => {
            if (worker.state === 'activated' || worker.state === 'redundant') {
              worker.removeEventListener('statechange', changed);
              if (worker.state === 'activated') resolve();
              else reject(new Error('Installation interrupted'));
            }
          };
          worker.addEventListener('statechange', changed);
          changed();
        });
      }
      
      const ready = await new Promise<boolean>((resolve): void => {
        const channel = new MessageChannel();
        const finish = (value: boolean): void => {
          clearTimeout(timer);
          channel.port1.close();
          resolve(value);
        };
        const timer = setTimeout((): void => finish(false), 60000);
        channel.port1.onmessage = (event): void => finish(event.data === true);
        worker.postMessage('prepare-offline', [channel.port2]);
      });
      status = ready ? 'ready' : 'failed';
    } catch {
      status = 'failed';
    }
  }

  function connectionChanged(): void {
    offline = !navigator.onLine;
  }

  onMount((): void => {
    connectionChanged();
    if (!dev && 'serviceWorker' in navigator) void prepare();
  });
</script>

<svelte:window ononline={connectionChanged} onoffline={connectionChanged} />

{#if offline}
  <p role="status">Offline · Account features need a connection.</p>
{/if}

{#if status === 'ready'}
  <p>Ready to reopen offline</p>
{:else if status === 'preparing'}
  <p role="status">Preparing offline access…</p>
{:else if status === 'failed'}
  <p role="status">Offline setup incomplete. Reconnect and try again.</p>
  <button class="secondary small" onclick={prepare}>Retry offline setup</button>
{/if}
