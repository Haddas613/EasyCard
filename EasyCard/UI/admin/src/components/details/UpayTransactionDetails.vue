<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t('UpayTransactionDetails')}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('CashieriD')}}</p>
          <p>{{model.cashieriD || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('TotalAmount')}}</p>
          <p>{{model.totalAmount || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('CreditCardCompanyCode')}}</p>
          <p>{{model.creditCardCompanyCode || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('MerchantNumber')}}</p>
          <p>{{model.merchantNumber || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('SessionID')}}</p>
          <p>{{model.sessionID || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('ErrorMessage')}}</p>
          <p>{{model.errorMessage || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('WebUrl')}}</p>
          <p>{{model.webUrl || '-'}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('ErrorDescription')}}</p>
          <p>{{model.errorDescription || '-'}}</p>
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