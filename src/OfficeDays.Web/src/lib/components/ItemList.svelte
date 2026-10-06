<script lang="ts">
  import type { Snippet } from 'svelte';
  let { children, loading = false, scrollable = false }: {
    children: Snippet; loading?: boolean; scrollable?: boolean;
  } = $props();
</script>

<ul class="item-list" class:scrollable aria-busy={loading}>
  {@render children()}
</ul>

<style>
  .item-list { margin: 1rem 0 0; padding: 0; list-style: none; }
  .item-list :global(li) { display: flex; align-items: center; justify-content: space-between; gap: .7rem; padding: .65rem .1rem; border-top: 1px solid var(--subtle-line); }
  .item-list :global(li:first-child) { border-top: 0; }
  .item-list :global(.item-main) { min-width: 0; }
  .item-list :global(:where(.item-main strong)), .item-list :global(:where(.item-main span)) { display: block; overflow-wrap: anywhere; }
  .item-list :global(.item-main small) { color: var(--muted); }
  .item-list :global(.empty) { color: var(--muted); font-size: .9rem; }
  @media (max-width: 650px) {
    .item-list :global(li) { flex-wrap: wrap; }
    .item-list :global(.item-main) { flex: 1; }
  }
  .scrollable { max-height: 25rem; overflow-y: auto; scrollbar-gutter: stable; }
</style>
