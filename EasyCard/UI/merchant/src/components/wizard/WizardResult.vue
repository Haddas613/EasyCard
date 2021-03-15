<template>
  <v-container fill-height fluid>
    <v-row align="center" justify="center">
      <v-col class="text-center caption">
        <template v-if="errors && errors.length > 0">
          <v-icon class="error--text font-weight-thin" size="170">mdi-progress-close</v-icon>
          <p class="subtitle-1 error--text">{{$t('Error')}}</p>
          <v-flex class="pt-1" flat>
            <template v-if="errors && errors.length > 0">
              <v-list-item class="px-0" v-for="e in errors" v-bind:key="e.message">
                <v-list-item-content>
                  <v-list-item-title v-if="e.code">
                    <v-badge class="my-0" color="error" inline :content="e.code"></v-badge>
                  </v-list-item-title>
                  <p class="pt-1 px-2 body-2">{{e.description}}</p>
                </v-list-item-content>
              </v-list-item>
            </template>
            <v-list-item v-if="!errors || errors.length === 0">
              <v-list-item-content>
                <p class="pt-1 px-2 body-2">{{$t('SomethingWentWrong')}}</p>
              </v-list-item-content>
            </v-list-item>
          </v-flex>
        </template>
        <template v-else>
          <slot></slot>
        </template>
        <template v-if="hasSlot('link')">
          <slot name="link"></slot>
        </template>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  props: {
    errors: {
      type: Array,
      default: []
    }
  },
  methods: {
    hasSlot(name = "default") {
      return !!this.$slots[name] || !!this.$scopedSlots[name];
    }
  },
};
</script>