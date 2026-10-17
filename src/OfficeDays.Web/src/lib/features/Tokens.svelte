<script lang="ts">
  import ItemList from '$lib/components/ItemList.svelte';
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import { displayTime } from '$lib/dates';
  import type { ApiToken } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  
  let tokens = $state<ApiToken[]>([]); 
  let editingId = $state<string | null>(null);
  let editedName = $state('');
  let name = $state(''); 
  let rawToken = $state(''); 
  let copied = $state(false);
  let loading = $state(true); 
  let busy = $state(false);
  let error = $state(''); 
  let message = $state('');
  
  async function load(): Promise<void> {
    loading = true; error = '';
    try { 
      tokens = await request<ApiToken[]>('/api/tokens'); 
    } catch (failure) { 
      error = errorMessage(failure); 
    } finally { 
      loading = false; 
    }
  }
  
  onMount(() => { void load(); });
  
  async function create(event: SubmitEvent): Promise<void> {
    event.preventDefault(); 
    busy = true; 
    error = '';
    message = '';
    try {
      const result = await mutate<{ token: string }>('/api/tokens', 'POST', { name });
      rawToken = result.token; 
      copied = false; 
      name = ''; 
      message = 'Token created. Copy it before leaving this page.'; 
      
      await load();
    } catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      busy = false; 
    }
  }
  
  function edit(token: ApiToken): void {
    editingId = token.id;
    editedName = token.name;
    error = '';
    message = '';
  }

  async function rename(event: SubmitEvent, token: ApiToken): Promise<void> {
    event.preventDefault();
    busy = true;
    error = '';
    message = '';
    try {
      const name = editedName.trim();
      await mutate(`/api/tokens/${token.id}`, 'PATCH', { name });
      tokens = tokens.map(item => item.id === token.id ? { ...item, name } : item);
      editingId = null;
      message = 'Token renamed.';
    } catch (failure) {
      error = errorMessage(failure);
    } finally {
      busy = false;
    }
  }

  async function revoke(token: ApiToken) : Promise<void> {
    if (!confirm(`Revoke “${token.name}”?`)) 
      return;
    
    busy = true; 
    error = ''; 
    message = '';
    
    try { 
      await mutate(`/api/tokens/${token.id}`, 'DELETE'); 
      message = 'Token revoked.'; 
      
      await load(); 
    }
    catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      busy = false; 
    }
  }
  
  async function copy(): Promise<void> {
    try { 
      await navigator.clipboard.writeText(rawToken); 
      copied = true; 
    }
    catch { 
      error = 'Could not copy automatically. Select and copy the token below.'; 
    }
  }
</script>

<section class="card">
  <p class="muted">
    For iOS Shortcuts and other trusted clients.
  </p>
  
  <form class="inline-form token-form" onsubmit={create}>
    <input maxlength="100" placeholder="e.g. Andrew's iPhone" required aria-label="Token name" bind:value={name} />
    <button type="submit" disabled={busy}>Create token</button>
  </form>
  {#if rawToken}
    <aside class="token-reveal">
      <strong>Copy this token now. It cannot be retrieved again.</strong>
      <div class="copy-row">
        <code>{rawToken}</code>
        <button class="secondary" onclick={copy}>
          {copied ? 'Copied' : 'Copy'}
        </button>
      </div>
    </aside>
  {/if}
  
  <Feedback {error} {message} />
  
  {#if error}
    <button class="secondary small" onclick={load} disabled={loading}>
      Retry tokens
    </button>
  {/if}
  
  <ItemList {loading}>
    {#if loading && !tokens.length}
      <li class="empty">Loading tokens…</li>
    {:else}
      {#each tokens as token (token.id)}
      <li>
        <div class="item-main">
          {#if editingId === token.id}
            <form class="rename-form" onsubmit={(event) => rename(event, token)}>
              <input aria-label="New token name" required maxlength="100" bind:value={editedName} disabled={busy} />
              <button type="submit" disabled={busy || !editedName.trim()}>Save</button>
              <button type="button" class="secondary" disabled={busy} onclick={() => editingId = null}>Cancel</button>
            </form>
          {:else}
            <strong>{token.name}</strong>
          {/if}
          <small>
            Created {displayTime(token.createdAt)} · Last used {displayTime(token.lastUsedAt)}
          </small>
          <span class="status-pill" class:revoked={token.status === 'Revoked'}>
            {token.status}
          </span>
        </div>
        <div class="actions">
          {#if editingId !== token.id}
            <button class="secondary small" disabled={busy} onclick={() => edit(token)} aria-label={`Rename ${token.name}`}>
              Rename
            </button>
          {/if}
          {#if token.status !== 'Revoked'}
            <button class="danger small" disabled={busy} onclick={() => revoke(token)}>
              Revoke
            </button>
          {/if}
        </div>
      </li>
    {:else}<li class="empty">No API tokens yet.</li>{/each}{/if}
  </ItemList>
</section>

<style>
  .actions {
    display: flex;
    align-items: center;
    gap: .25rem;
    flex-shrink: 0;
  }

  .rename-form {
    display: flex;
    flex-wrap: wrap;
    gap: .55rem;
  }

  .rename-form input {
    flex: 1 1 10rem;
    min-width: 0;
  }

  .item-main {
    flex: 1;
  }

  .copy-row { 
    display: flex;
    align-items: center; 
    gap: .7rem;
  }
  
  .token-reveal { 
    margin-top: 1rem; 
    padding: .9rem; 
    border: 1px solid var(--warning-border); 
    border-radius: .65rem; 
    background: var(--warning);
  }
  
  .copy-row { 
    margin-top: .65rem; 
  }
  
  .copy-row code { 
    flex: 1; 
    padding: .6rem; 
    overflow-wrap: anywhere; 
    border-radius: .35rem; 
    background: var(--input); 
  }
  
  .status-pill {
    display: inline-block; 
    margin-top: .2rem;
    margin-left: .2rem;
    padding: .12rem .4rem; 
    border-radius: 999px; 
    color: var(--green-dark); 
    background: var(--green-soft); 
    font-size: .72rem; 
  }
  
  .status-pill.revoked { 
    color: var(--muted); 
    background: var(--revoked); 
  }
  
  .token-form { 
    margin-top: 1rem; 
  }
  
  .copy-row code { 
    min-width: 0; 
  }
  
  @media (max-width: 650px) {
    .copy-row { flex-wrap: wrap; }
    .copy-row code { flex-basis: 100%; }
  }
</style>
