<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t(title)}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="6" md="3" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('VAT')}}</p>
          <p>{{(model.vatRate * 100).toFixed(0)}}%</p>
        </v-col>
        <v-col cols="6" md="3" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('VATAmount')}}</p>
          <p>{{model.vatTotal | currency(currency || model.$currency || model.currency)}}</p>
        </v-col>
        <v-col cols="6" md="3" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('NetAmount')}}</p>
          <p>{{model.netTotal | currency(currency || model.$currency || model.currency)}}</p>
        </v-col>
        <v-col cols="6" md="3" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Amount')}}</p>
          <p>{{model[amountKey] | currency(currency || model.$currency || model.currency)}}</p>
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
    },
    amountKey: {
      type: String,
      default: "totalAmount"
    },
    title: {
      type: String,
      default: "AmountDetails"
    },
    currency: {
      type: String,
      default: null
    }
  }
};
</script>