<template>
  <div>
    <v-divider></v-divider>
    <v-list-item v-on="clickable ? {click: () => $emit('click')} : {}" :ripple="false" class="px-0" dense="dense">
      <v-list-item-action v-if="hasSlot('prepend')" class="px-1">
        <slot name="prepend"></slot>
      </v-list-item-action>
      <v-list-item-content>
        <v-row no-gutters>
          <v-col v-if="hasSlot('left')" class="px-1">
            <slot name="left"></slot>
          </v-col>
          <v-col v-if="hasSlot('right')" class="text-end">
            <slot name="right"></slot>
          </v-col>
        </v-row>
      </v-list-item-content>
      <v-list-item-action v-if="hasSlot('append')">
        <slot name="append"></slot>
      </v-list-item-action>
    </v-list-item>
    <v-divider></v-divider>
  </div>
</template>

<script>
export default {
    props: {
      clickable: {
        default: true
      }
    },
  methods: {
    hasSlot(name) {
      return !!this.$slots[name] || !!this.$scopedSlots[name];
    }
  }
};
</script>

<style lang="scss" scoped>
.ec-dialog-invoker {
  background-color: white;
}
</style>