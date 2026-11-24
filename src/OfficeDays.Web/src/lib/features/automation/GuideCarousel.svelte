<script lang="ts">
  import { iosSteps, screenshotUrl } from './iosSteps';

  let index = $state(0);
  const step = $derived(iosSteps[index]);
</script>

<section class="card guide" aria-label="Shortcut setup instructions">
  <h2>Setup guide</h2>
  <div class="controls">
    <button type="button" class="secondary small" disabled={index === 0} onclick={(): void => { index--; }}>Previous</button>
    <span class="muted">Step {index + 1} of {iosSteps.length}</span>
    <button type="button" class="secondary small" disabled={index === iosSteps.length - 1} onclick={(): void => { index++; }}>Next</button>
  </div>
  
  <figure aria-live="polite" aria-atomic="true">
    {#each iosSteps as slide, i}
      {#if slide.image}
        <a hidden={i !== index} href={screenshotUrl(slide.image)} target="_blank" rel="noreferrer" aria-label={'Enlarge screenshot: ' + slide.title}>
          <img src={screenshotUrl(slide.image)} alt={slide.alt} loading="eager" decoding="async" />
        </a>
      {/if}
    {/each}
    
    <figcaption>
      <strong>{step.title}</strong>
      <p>{step.instruction}</p>
    </figcaption>
  </figure>
  
  <details>
    <summary>Set up an arrival automation</summary>
    <ol>
      <li>In Shortcuts, open Automation and tap the plus button.</li>
      <li>Choose Arrive, select your office, and adjust the radius around it.</li>
      <li>Choose Run Immediately if available, then select the OfficeDays shortcut.</li>
      <li>Save the automation and check that it’s enabled.</li>
    </ol>
  </details>
</section>

<style>
  .controls { 
    display: flex; 
    align-items: center; 
    justify-content: space-between; 
    gap: .3rem; 
    margin-block: .75rem; 
  }
  
  figure { 
    margin: 0; 
  }
  
  figure a { 
    display: grid; 
    place-items: center; 
    background: var(--ground); 
    border-radius: .6rem; 
    padding: .5rem; 
  }
  
  figure a[hidden] { 
    display: none; 
  }
  
  img { 
    display: block; 
    width: auto; 
    max-width: 100%; 
    height: auto; 
    max-height: 24rem; 
    border-radius: .45rem; 
  }
  
  figcaption { 
    margin-top: .75rem; 
  }
  
  figcaption p { 
    margin: .25rem 0 .75rem; 
  }
  
  details { 
    border-top: 1px solid var(--line); 
  }
  
  summary { 
    padding-block: .7rem; 
    font-weight: 650; 
    cursor: pointer; 
  }
  
  ol { 
    padding-left: 1.3rem; 
    margin: 0; 
  }
</style>
