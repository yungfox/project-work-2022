<template>
    <div class="background">
        <div class="mask">
            <div class="row" style="justify-content: center">
                <div class="content">
                    <home-page></home-page>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import HomePage from './components/HomePage'
export default {
    components: {
        HomePage
    },
}
</script>