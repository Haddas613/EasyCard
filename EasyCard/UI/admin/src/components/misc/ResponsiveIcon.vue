<template>
  <v-icon v-bind:class="{'invert': $vuetify.rtl}">
    <slot></slot>
  </v-icon>
</template>

<script>
export default {
    name: "ResponsiveIcon"
};
</script>
<style lang="scss" scoped>
.invert{
    transform: rotate(180deg);
}
</style>