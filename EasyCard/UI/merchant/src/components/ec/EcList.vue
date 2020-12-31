<template>
  <v-list :two-line="!dense" :dense="dense" subheader class="py-0 fill-height" :color="color">
    <v-list-item v-for="(item, index) in items" :key="index" v-on="clickable ? {click: () => onclick(item)} : {}" v-bind:class="{'px-0': dense, 'dashed': dashed}">
      <v-list-item-action v-if="hasSlot('prepend')" v-bind:class="{'col-unset': $vuetify.breakpoint.mdAndUp}">
        <slot v-bind:item="item" v-bind:index="index" name="prepend"></slot>
      </v-list-item-action>
      <v-list-item-content>
        <v-row no-gutters :class="{'col-reverse' : $vuetify.breakpoint.smAndDown, 'w50': !stretch}">
          <slot v-bind:item="item" v-bind:index="index" name="left"></slot>
        </v-row>
        <v-row no-gutters :class="{'col-reverse' : $vuetify.breakpoint.smAndDown, 'w50': !stretch}" v-if="hasSlot('right')">
          <slot v-bind:item="item" v-bind:index="index" name="right"></slot>
        </v-row>
      </v-list-item-content>
      <v-list-item-action v-bind:class="{'col-unset': $vuetify.breakpoint.mdAndUp}">
        <slot v-bind:item="item" v-bind:index="index" name="append"></slot>
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
    },
    dashed: {
      type: Boolean,
      default: false,
      required: false
    },
    color: {
      type: String,
      default: "white"
    },
    stretch: {
      type: Boolean, 
      default: false
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
.col-unset{
  flex-direction: unset;
}
.dashed:not(:last-child) {
  border-bottom: 1px solid var(--v-eclgray-base);
}
.w50{
  width:50%;
}
</style>