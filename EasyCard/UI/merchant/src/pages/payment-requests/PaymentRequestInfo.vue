<template>
  <v-flex>
    <div v-if="model">
      <v-card flat class="mb-2">
        <v-card-title class="py-3 ecdgray--text subtitle-2 text-uppercase">{{$t('GeneralInfo')}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text class="body-1 black--text" v-if="model">
          <v-row class="info-container">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ID')}}</p>
              <v-chip color="primary" small>{{model.$paymentRequestID | guid}}</v-chip>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Terminal')}}</p>
              <p>{{terminalName}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('TransactionID')}}</p>
              <router-link
                v-if="model.paymentTransactionID"
                class="primary--text"
                link
                :to="{name: 'Transaction', params: {id: model.paymentTransactionID}}"
              >
                <small>{{(model.dealDetails.consumerID || '-') | guid}}</small>
              </router-link>
              <p v-else>-</p>
            </v-col>
          </v-row>
          <v-row class="info-container">
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Created')}}</p>
              <p>{{model.$paymentRequestTimestamp | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('DueDate')}}</p>
              <p>{{model.$dueDate | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Amount')}}</p>
              <p>{{model.paymentRequestAmount | currency(model.$currency)}}</p>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      <v-card flat class="my-2">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t("TransactionDetails")}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text">
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('VAT')}}</p>
              <p>{{model.vatRate * 100}}%</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('VATAmount')}}</p>
              <p>{{model.vatTotal | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('NetAmount')}}</p>
              <p>{{model.netTotal | currency(model.$currency)}}</p>
            </v-col>
            <v-col cols="6" md="3" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Amount')}}</p>
              <p>{{model.paymentRequestAmount | currency(model.$currency)}}</p>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      <v-card flat class="my-2" v-if="model.dealDetails && model.dealDetails.items.length > 0">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t("Items")}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <transaction-items-list
            :items="model.dealDetails.items"
          ></transaction-items-list>
        </v-card-text>
      </v-card>
      <v-card flat class="my-2">
        <v-card-title
          class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
        >{{$t("CreditCardDetails")}}</v-card-title>
        <v-divider></v-divider>
        <v-card-text>
          <v-row class="info-container body-1 black--text" v-if="model">
            <template v-if="model.dealDetails.consumerID">
              <v-col cols="12" md="4" class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('CustomerID')}}</p>
                <router-link
                  class="primary--text"
                  link
                  :to="{name: 'Customer', params: {id: model.dealDetails.consumerID}}"
                >
                  <small>{{(model.dealDetails.consumerID || '-') | guid}}</small>
                </router-link>
              </v-col>
              <v-col cols="12" md="4" class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('CustomerEmail')}}</p>
                <p>{{(model.dealDetails.consumerEmail || '-')}}</p>
              </v-col>
              <v-col cols="12" md="4" class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('CustomerPhone')}}</p>
                <p>{{(model.dealDetails.consumerPhone || '-')}}</p>
              </v-col>
            </template>
          </v-row>
        </v-card-text>
      </v-card>
    </div>
  </v-flex>
</template>

<script>
export default {
  components: {
    TransactionItemsList: () => import("../../components/transactions/TransactionItemsList"),
  },
  data() {
    return {
      model: null,
      terminalName: "-",
      numberOfRecords: 0
    };
  },
  async mounted() {
    this.model = await this.$api.paymentRequests.getPaymentRequest(
      this.$route.params.id
    );

    if (!this.model) {
      return this.$router.push("/admin/payment-requests/list");
    }

    let terminals = (await this.$api.terminals.getTerminals()).data;
    let usedTerminal = this.lodash.find(
      terminals,
      t => t.terminalID == this.model.$terminalID
    );
    if (usedTerminal) {
      this.terminalName = usedTerminal.label;
    } else {
      this.terminalName = this.$t("NotAccessible");
    }
  }
};
</script>