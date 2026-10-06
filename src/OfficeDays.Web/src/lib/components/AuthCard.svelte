<script lang="ts">
  import type { Snippet } from 'svelte';
  let { children, registration = false }: { children: Snippet; registration?: boolean } = $props();
</script>

<section class="login-card" class:register-card={registration}>
  <div class="brand-mark">OD</div>
  {@render children()}
</section>

<style>
  .login-card { border: 1px solid var(--line); border-radius: .85rem; background: var(--paper); box-shadow: 0 2px 8px var(--shadow); }
  .login-card { width: min(390px, 100%); margin: 14vh auto; padding: 2rem; }
  .brand-mark { display: grid; place-items: center; width: 2.6rem; height: 2.6rem; margin-bottom: 1.2rem; border-radius: .7rem; color: var(--on-green); background: var(--green); font-weight: 800; }
  .login-card :global(a) { color: var(--green-dark); }
  .register-card { margin-top: 2rem; margin-bottom: 2rem; }
  @media (max-width: 650px) {
    .login-card { padding: 1.3rem; margin-top: 2rem; }
  }
</style>
