<template>
  <v-list :two-line="!dense" :dense="dense" subheader class="py-0 fill-height">
    <v-list-item v-for="(item, index) in items" :key="index" v-on="clickable ? {click: () => onclick(item)} : {}">
      <v-list-item-action v-if="hasSlot('prepend')">
        <slot v-bind:item="item" name="prepend"></slot>
      </v-list-item-action>
      <v-list-item-content>
        <v-row no-gutters style="width:50%;" :class="{'col-reverse' : $vuetify.breakpoint.smAndDown}">
          <slot v-bind:item="item" name="left"></slot>
        </v-row>
        <v-row no-gutters style="width:50%;" :class="{'col-reverse' : $vuetify.breakpoint.smAndDown}">
          <slot v-bind:item="item" name="right"></slot>
        </v-row>
      </v-list-item-content>
      <v-list-item-action v-if="hasSlot('append')">
        <slot v-bind:item="item" name="append"></slot>
      </v-list-item-action>
    </v-list-item>
  </v-list>
</template>

<script>
export default {
  props: {
    items: {
      type: Array,
      default: [],
    },
    dense: {
      type: Boolean,
      default: false,
      required: false
    },
    clickable: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  methods: {
    hasSlot(name) {
      return !!this.$slots[name] || !!this.$scopedSlots[name];
    },
    onclick(item){
      this.$emit('click', item);
    }
  }
};
</script>

<style lang="scss" scoped>
.col-reverse{
  flex-direction: column-reverse;
}
</style>