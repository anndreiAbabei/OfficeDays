<script lang="ts">
  import type { Appearance } from '$lib/types';

  let { value = $bindable() }: { value: Appearance } = $props();
</script>

<label>
  Appearance
  <select bind:value>
    <option value="System">System</option>
    <option value="Light">Light</option>
    <option value="Dark">Dark</option>
  </select>
</label>
