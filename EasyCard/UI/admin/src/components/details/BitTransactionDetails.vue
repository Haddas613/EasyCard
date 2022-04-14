<template>
  <v-card flat class="my-2">
    <template>
      <v-card-title
        class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
      >{{$t("BitDetails")}}</v-card-title>
      <v-divider></v-divider>
    </template>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('PaymentInitiationId')}}</p>
          <p>
            <p>{{model.bitPaymentInitiationId || '-'}}</p>
          </p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('BitTransactionSerialId')}}</p>
          <p>{{model.bitTransactionSerialId || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('RequestStatusCode')}}</p>
          <p>{{model.requestStatusCode || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('RequestStatusDescription')}}</p>
          <p>{{model.requestStatusDescription || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('BitMerchantNumber')}}</p>
          <p>{{model.bitMerchantNumber || '-'}}</p>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  components: {
    BankDetailsString: () => import("../bank/BankDetailsString"),
  },
  props: {
    model: {
      type: Object,
      required: true
    },
  }
};
</script>