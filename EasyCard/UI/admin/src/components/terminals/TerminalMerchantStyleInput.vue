<template>
  <v-file-input
    show-size
    truncate-length="15"
    accept="text/css"
    :label="$t('CustomCSS')"
    @change="processCSS($event)"
  ></v-file-input>
</template>

<script>
export default {
  model: {
    prop: "data",
    event: "change"
  },
  props: {
    data: {
      type: Object,
      default: () => null,
    }
  },
  methods: {
    async processCSS(file) {
        if(!file || file.length === 0){
            return;
        }
        if(file.type.indexOf("text/css") < 0){
            return this.$toasted.show(i18n.t('OnlyCSSFilesAreAllowed'), { type: 'error' });
        }
        if(file.size > 1000000){
            return this.$toasted.show(i18n.t('MaxFileSizeIs1MB'), { type: 'error' });
        }
        
        let operation = await this.$api.terminals.uploadCustomCSS(this.data.terminalID, file);
        if(!this.$apiSuccess(operation)) return;

        this.data.checkoutSettings.customCssReference = operation.additionalData.url;
        this.$emit("change", this.data);
    }
  },
};
</script>