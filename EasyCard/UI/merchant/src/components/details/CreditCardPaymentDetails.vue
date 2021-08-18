<template>
  <!-- <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t("CreditCardDetails")}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text> -->
      <v-row class="info-container body-1 black--text">
        <v-col cols="4" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('CardNumber')}}</p>
          <p>
            <span dir="ltr">{{model.cardNumber || '-'}}</span>
          </p>
        </v-col>
        <v-col cols="4" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('CardExpiration')}}</p>
          <p>{{model.cardExpiration || '-'}}</p>
        </v-col>
        <v-col cols="4" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('VoucherNumber')}}</p>
          <p>{{model.shovarNumber || '-'}}</p>
        </v-col>
      </v-row>
    <!-- </v-card-text>
  </v-card> -->
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