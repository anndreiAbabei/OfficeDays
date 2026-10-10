<script lang="ts">
    import { page } from '$app/state';
</script>
<section class="card">
    <h1>
        {page.status === 404 ? 'Page not found' : 'Unable to open this page'}
    </h1>
    <p>{page.error?.message}</p>
    <a href="/">Return to dashboard</a>
</section>
