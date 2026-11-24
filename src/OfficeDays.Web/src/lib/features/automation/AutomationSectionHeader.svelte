<script lang="ts">
  type Props = {
    heading: 'h1' | 'h2';
    title: string;
    description: string;
    icon: 'calendar' | 'code';
  };

  let { heading, title, description, icon }: Props = $props();
</script>

<header>
  <span class="icon" class:calendar={icon === 'calendar'} aria-hidden="true">
    {icon === 'calendar' ? '✓' : '</>'}
  </span>
  <div>
    <svelte:element this={heading} class="title">{title}</svelte:element>
    <p>{description}</p>
  </div>
</header>

<style>
  header {
    display: flex;
    align-items: center;
    gap: .85rem;
    margin-bottom: 1rem;
  }

  .title {
    margin: 0 0 .3rem;
    font-size: 2rem;
    letter-spacing: -.035em;
  }

  p { 
    margin: 0; 
  }

  .icon {
    flex: none;
    display: grid;
    place-items: center;
    width: 3rem;
    height: 3rem;
    border: 2px solid var(--green);
    border-radius: .55rem;
    background: var(--green-soft);
    color: var(--green-dark);
    font: 700 1.1rem/1 monospace;
  }

  .calendar {
    border-top-width: .6rem;
    font: inherit;
    font-size: 1.6rem;
    font-weight: 750;
    line-height: 1;
  }
</style>
