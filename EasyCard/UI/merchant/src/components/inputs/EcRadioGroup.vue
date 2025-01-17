<template>
  <v-list width="100%">
    <v-radio-group :mandatory="mandatory" @change="onSelect($event)" v-model.lazy="selected" v-if="items" :hide-details="!mandatory">
      <template v-for="(i, index) in items">
        <v-list-item v-bind:key="i[valuekey]">
          <v-list-item-content>
            <template v-if="hasSlot()">
              <slot v-bind:item="i"></slot>
            </template>
            <template v-else>{{i[labelkey]}}</template>
          </v-list-item-content>
          <v-list-item-action>
            <v-radio :value="i[valuekey]" color="black" :disabled="itemDisabledKey ? i[itemDisabledKey] : false"></v-radio>
          </v-list-item-action>
        </v-list-item>
        <v-divider v-if="index + 1 < items.length" :key="index"></v-divider>
      </template>
    </v-radio-group>
  </v-list>
</template>

<script>
export default {
  props: {
    data: {
      type: Array,
      default: () => [],
      required: true
    },
    labelkey: {
      type: String,
      default: "label"
    },
    valuekey: {
      type: String,
      default: "code"
    },
    mandatory: {
      type: Boolean,
      default: false
    },
    model: {
      required: false
    },
    returnObject: {
      type: Boolean,
      default: false
    },
    itemDisabledKey:{
      type: String,
      default: null,
      required: false
    }
  },
  data() {
    return {
      items: [...this.data],
      selected: null
    };
  },
  watch: {
    model(newValue, oldValue) {
      if (this.returnObject) {
        this.selected = newValue ? newValue[this.valuekey] : null;
      } else {
        this.selected = newValue;
      }
    }
  },
  mounted() {
    if (!this.model) return;
    //if we are in the object mode, we select object key as current value
    //because radio input can not work with object
    if (this.returnObject) {
      this.selected = this.model[this.valuekey];
    } else {
      this.selected = this.model;
    }
  },
  methods: {
    onSelect(val) {
      if (this.returnObject) {
        let obj = this.lodash.find(this.items, i => i[this.valuekey] == val);

        //Workaround, vuetify radio group returns 0 for null values
        if(!obj && !val){
          obj = this.lodash.find(this.items, i => i[this.valuekey] == null);
        }
        if (obj) this.$emit("update:model", obj);
      } else {
        this.$emit("update:model", val);
      }
      this.$emit("change", val);
    },
    hasSlot(name = "default") {
      return !!this.$slots[name] || !!this.$scopedSlots[name];
    }
  }
};
</script>

<style lang="scss" scoped>
</style>