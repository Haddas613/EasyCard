<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t('ClearingHouseTransactionDetails')}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('ClearingHouseTransactionID')}}</p>
          <p>{{model.clearingHouseTransactionID || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('MerchantReference')}}</p>
          <p>{{model.merchantReference || '-'}}</p>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  props: {
    model: {
      type: Object,
      required: true
    }
  }
};
</script>