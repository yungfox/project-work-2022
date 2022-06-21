<template>
    <div class="background">
        <div class="mask">
            <div class="row">
                <div class="sidebar">
                    <div class="menu-items-section">
                        <router-link to="/" class="link-reset">
                            <div class="menu-item">
                                <div class="menu-item-icon"></div>
                                <span class="menu-item-title">overview</span>
                            </div>
                        </router-link>
                        <router-link to="/details" class="link-reset">
                            <div class="menu-item">
                                <div class="menu-item-icon"></div>
                                <span class="menu-item-title">details</span>
                            </div>
                        </router-link>
                        <router-link to="/settings" class="link-reset">
                            <div class="menu-item">
                                <div class="menu-item-icon"></div>
                                <span class="menu-item-title">settings</span>
                            </div>
                        </router-link>
                    </div>
                </div>
                <div class="content">
                    <router-view />
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'App',
    mounted() {
        var connection = new signalR.HubConnectionBuilder().withUrl("/parkingHub").build();

        connection.on("ReceiveMessage", function (user, message) {
            //callback
        });

        connection.start().then(function () {
            console.log('connesso a signalr')
        }).catch(function (err) {
            return console.error(err.toString());
        });
    }
}
</script>
