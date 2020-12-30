<template>
  <v-icon v-bind:class="{'invert': $vuetify.rtl}" :small="small" :x-small="xSmall" :large="large" :x-large="xLarge">
    <slot></slot>
  </v-icon>
</template>

<script>
export default {
    props: {
      small: {
        type: Boolean,
        default: false,
      },
      xSmall: {
        type: Boolean,
        default: false,
      },
      large: {
        type: Boolean,
        default: false,
      },
      xLarge: {
        type: Boolean,
        default: false,
      },
    },
    name: "ResponsiveIcon"
};
</script>
<style lang="scss" scoped>
.invert{
    transform: rotate(180deg);
}
</style>