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
          <p class="caption ecgray--text text--darken-2">{{$t('CustomerName')}}</p>
          <p>{{(model.consumerName || '-')}}</p>
        </v-col>
        <v-col cols="12" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('NationalID')}}</p>
          <p>{{(model.consumerNationalID || '-')}}</p>
        </v-col>
        <template v-if="model.consumerID">
          <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('Customer')}}</p>
            <small v-if="consumerName">{{(consumerName)}}</small>
            <small v-else>{{model.consumerID | guid}}</small>
          </v-col>
          <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('CustomerEmail')}}</p>
            <p>{{(model.consumerEmail || '-')}}</p>
          </v-col>
          <v-col cols="12" md="4" class="info-block">
            <p class="caption ecgray--text text--darken-2">{{$t('CustomerPhone')}}</p>
            <p>{{(model.consumerPhone || '-')}}</p>
          </v-col>
          <v-col cols="12" md="4" class="info-block" v-if="model.consumerExternalReference">
            <p class="caption ecgray--text text--darken-2">{{$t('CustomerExternalReference')}}</p>
            <p>{{(model.consumerExternalReference || '-')}}</p>
          </v-col>
          <template v-if="model.consumerAddress">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Street')}}</p>
              <p>{{model.consumerAddress.street || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('House')}}</p>
              <p>{{model.consumerAddress.house || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Apartment')}}</p>
              <p>{{model.consumerAddress.apartment || '-'}}</p>
            </v-col>
          </template>
          <template v-if="model.responsiblePerson || model.externalUserID">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ResponsiblePerson')}}</p>
              <p>{{model.responsiblePerson || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ExternalUserID')}}</p>
              <p>{{model.externalUserID || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Branch')}}</p>
              <p>{{model.branch || '-'}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Department')}}</p>
              <p>{{model.department || '-'}}</p>
            </v-col>
          </template>
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