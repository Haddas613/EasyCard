<template>
  <v-container fill-height fluid>
    <v-row align="center" justify="center">
      <v-col class="text-center caption">
        <template v-if="errors && errors.length > 0 || error">
          <v-icon class="error--text font-weight-thin" size="170">mdi-progress-close</v-icon>
          <p class="subtitle-1 error--text">{{error || $t('Error')}}</p>
          <v-flex class="pt-1" flat>
            <v-card flat v-if="errors && errors.length > 0">
              <v-card-text>
                <v-list two-line subheader>
                  <v-list-item class="px-0" v-for="e in errors" dashed v-bind:key="e.message">
                    <v-list-item-content>
                      <v-list-item-title>
                        <v-badge v-if="e.code" class="my-0" color="error" inline :content="e.code"></v-badge>
                      </v-list-item-title>
                      <v-list-item-subtitle v-text="e.description"></v-list-item-subtitle>
                    </v-list-item-content>
                  </v-list-item>
                </v-list>
              </v-card-text>
            </v-card>

            <v-list-item v-if="(!errors || errors.length === 0) && !error">
              <v-list-item-content>
                <p class="pt-1 px-2 body-2">{{$t('SomethingWentWrong')}}</p>
              </v-list-item-content>
            </v-list-item>
          </v-flex>
          <slot name="errors"></slot>
        </template>
        <template v-else>
          <slot></slot>
        </template>
        <template v-if="hasSlot('slip')">
          <slot name="slip"></slot>
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
    },
    error: {
      type: String,
      default: "",
      required: false
    }
  },
  methods: {
    hasSlot(name = "default") {
      return !!this.$slots[name] || !!this.$scopedSlots[name];
    }
  },
};
</script>