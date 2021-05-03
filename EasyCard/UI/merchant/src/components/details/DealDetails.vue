<template>
  <v-card flat class="my-2">
    <v-card-title
      class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
    >{{$t("DealDetails")}}</v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <v-row class="info-container body-1 black--text">
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('DealDescription')}}</p>
          <p>{{(model.dealDescription || '-')}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('CustomerEmail')}}</p>
            <p>{{(model.consumerEmail || '-')}}</p>
          </v-col>
          <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('CustomerPhone')}}</p>
            <p>{{(model.consumerPhone || '-')}}</p>
          </v-col>
        <template v-if="model.consumerID">
          <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('Customer')}}</p>
            <router-link
              class="primary--text"
              link
              :to="{name: 'Customer', params: {id: model.consumerID}}"
            >
              <small v-if="consumerName">{{(consumerName)}}</small>
              <small v-else>{{model.consumerID | guid}}</small>
            </router-link>
          </v-col>
        </template>
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
    consumerName: {
        type: String,
        required: false,
        default: null
    }
  }
};
</script>