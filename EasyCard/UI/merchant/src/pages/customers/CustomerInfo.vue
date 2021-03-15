<template>
  <v-flex>
    <v-card class="mx-2 my-2">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('PersonalInformation')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn
              text
              class="primary--text px-0"
              link
              :to="{name: 'EditCustomer', params: { id: this.$route.params.id}}"
            >
              <v-icon left class="body-1">mdi-pencil-outline</v-icon>
              {{$t('Edit')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Name')}}</p>
          <p>{{model.consumerName}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Phone')}}</p>
          <p class="primary--text">{{model.consumerPhone}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Email')}}</p>
          <p class="primary--text">{{model.consumerEmail}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('NationalID')}}</p>
          <p>{{model.consumerNationalID}}</p>
        </div>
        <div class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Address')}}</p>
          <p>{{model.consumerAddress}}</p>
        </div>
      </v-card-text>
    </v-card>

    <v-card class="mx-2 my-2" :loading="tokens == null">
      <v-card-title class="py-2">
        <v-row no-gutters class="py-0">
          <v-col cols="9" class="d-flex">
            <span class="pt-2 ecdgray--text subtitle-2 text-uppercase">{{$t('SavedCards')}}</span>
          </v-col>
          <v-col cols="3" class="d-flex justify-end">
            <v-btn
              text
              class="primary--text px-0"
              link
              :to="{name: 'CreateCardToken', params: { customerid: this.$route.params.id}}"
            >
              <v-icon left class="body-1">mdi-plus-circle</v-icon>
              {{$t('Add')}}
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text class="body-1 black--text">
        <ec-list :items="tokens" v-if="tokens && tokens.length > 0">

          <template v-slot:left="{ item }">
            <v-col cols="12" md="6" lg="6" class="caption ecgray--text">{{item.creditCardTokenID | guid}}</v-col>
            <v-col cols="12" md="6" lg="6">
              <span v-bind:class="{'error--text': item.expired}">
                {{item.cardNumber}}  <small class="px-1">{{item.cardExpiration}}</small>
              </span>
            </v-col>
          </template>

          <template v-slot:right="{ item }">
            <v-col cols="12" md="6" lg="6" class="text-end"></v-col>
            <v-col cols="12" md="6" lg="6" class="text-end">
              {{item.cardOwnerName}}
            </v-col>
          </template>

          <template v-slot:append="{ item }">
            <v-btn icon @click="deleteCardToken(item.creditCardTokenID)">
              <v-icon class="red--text">mdi-delete</v-icon>
            </v-btn>
          </template>
        </ec-list>
        <p class="subtitle-2 ecgray--text" v-if="tokens && tokens.length === 0">
          {{$t("NothingToShow")}}
        </p>
      </v-card-text>
    </v-card>
  </v-flex>
</template> 

<script>
import EcList from "../../components/ec/EcList";

export default {
  components: {
    EcList,
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: {},
      tokens: null
    };
  },
  methods: {
    async deleteCustomer() {
      let result = await this.$api.consumers.deleteConsumer(
        this.$route.params.id
      );
      this.$router.push({ name: "Customers" });
    },
    async deleteCardToken(tokenId) {
      let result = await this.$api.cardTokens.deleteCardToken(tokenId);

      //server errors will be displayed automatically
      if (!result) return;
      if (result.status === "success") {
        this.tokens =
          (
            await this.$api.cardTokens.getCustomerCardTokens(
              this.$route.params.id
            )
          ).data || [];
      } else {
        this.$toasted.show(result.message, { type: "error" });
      }
    }
  },
  async mounted() {
    if (this.data) {
      this.model = this.data;
      return;
    }

    let customer = await this.$api.consumers.getConsumer(this.$route.params.id);

    if (!customer) {
      return this.$router.push("/admin/customers/list");
    }
    this.model = customer;

    this.tokens =
      (await this.$api.cardTokens.getCustomerCardTokens(this.$route.params.id))
        .data || [];

    this.$store.commit("ui/changeHeader", {
      value: {
        threeDotMenu: [
          {
            text: this.$t("CreateCustomer"),
            fn: () => {
              this.$router.push({ name: "CreateCustomer" });
            }
          },
          {
            text: this.$t("EditCustomer"),
            fn: () => {
              this.$router.push({
                name: "EditCustomer",
                id: this.$route.params.id
              });
            }
          },
          {
            text: this.$t("Charge"),
            fn: () => {
              this.$store.commit("payment/addLastChargedCustomer", {
                customerID: this.$route.params.id,
                terminalID: this.model.terminalID
              });
              this.$router.push({
                name: "Charge",
                params: { customerid: this.$route.params.id }
              });
            }
          },
          {
            text: this.$t("Refund"),
            fn: () => {
              this.$router.push({
                name: "Refund",
                params: { customerid: this.$route.params.id }
              });
            }
          },
          {
            text: this.$t("Transactions"),
            fn: () => {
              this.$router.push({
                name: "TransactionsFiltered",
                params: { filters: {
                  consumerID: this.$route.params.id
                } }
              });
            }
          },
          {
            text: this.$t("BillingDeals"),
            fn: () => {
              this.$router.push({
                name: "BillingDeals",
                params: { filters: {
                  consumerID: this.$route.params.id
                } }
              });
            }
          },
          {
            text: this.$t("Invoices"),
            fn: () => {
              this.$router.push({
                name: "Invoices",
                params: { filters: {
                  consumerID: this.$route.params.id
                } }
              });
            }
          },
          {
            text: this.$t("PaymentRequests"),
            fn: () => {
              this.$router.push({
                name: "PaymentRequests",
                params: { filters: {
                  consumerID: this.$route.params.id
                } }
              });
            }
          },
          {
            text: this.$t("DeleteCustomer"),
            fn: this.deleteCustomer.bind(this)
          }
        ],
        text: { translate: false, value: this.model.consumerName }
      }
    });
  }
};
</script>

<style lang="scss" scoped>

</style>