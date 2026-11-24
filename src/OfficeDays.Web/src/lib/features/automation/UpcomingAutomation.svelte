<script lang="ts">
  let { platform }: { platform: string } = $props();
</script>

<a href="/automation">← Automation</a>
<section class="card">
  <h1>{platform} automation</h1>
  <p class="muted">Not yet available. We’re working on it!</p>
</section>

<style>
  section {
    margin-top: 1.25rem;
    padding: clamp(1.25rem, 3vw, 2rem);
  }

  h1 { 
    margin-bottom: .85rem; 
  }
</style>
