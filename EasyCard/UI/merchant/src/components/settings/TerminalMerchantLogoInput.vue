<template>
  <v-file-input
    show-size
    truncate-length="15"
    accept="image/*"
    :label="$t('MerchantLogo')"
    @change="processImage($event)"
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
    async processImage(file) {
        if(!file || file.length === 0){
            return;
        }
        if(file.type.indexOf("image/") < 0){
            return this.$toasted.show(i18n.t('OnlyImagesAreAllowed'), { type: 'error' });
        }
        if(file.size > 1000000){
            return this.$toasted.show(i18n.t('MaxFileSizeIs1MB'), { type: 'error' });
        }
        
        let operation = await this.$api.terminals.uploadMerchantLogo(this.data.terminalID, file);
        if(!this.$apiSuccess(operation)) return;

        this.data.paymentRequestSettings.merchantLogo = operation.additionalData.url;
        this.$emit("change", this.data);
    }
  },
};
</script>