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
              <small>{{model.$invoiceID}}</small>
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
              <p>{{model.$invoiceTimestamp | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('InvoiceDate')}}</p>
              <p>{{model.$invoiceDate | ecdate('LLLL')}}</p>
            </v-col>
            <v-col cols="12" md="4" class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Amount')}}</p>
              <p>{{model.invoiceAmount | currency(model.$currency)}}</p>
            </v-col>
          </v-row>
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
  data() {
    return {
      model: null,
      terminalName: "-",
      transactions: null,
      transactionsFilter: {
        take: 100,
        skip: 0,
        invoiceID: this.$route.params.id
      },
      numberOfRecords: 0
    };
  },
  async mounted() {
    this.model = await this.$api.invoicing.getInvoice(this.$route.params.id);

    if (!this.model) {
      return this.$router.push("/admin/billing-deals/list");
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