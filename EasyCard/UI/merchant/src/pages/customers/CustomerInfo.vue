<template>
  <v-flex>
    <customer-delete-dialog :show.sync="deleteCustomerDialog" @ok="deleteCustomer()"></customer-delete-dialog>
    <v-card flat class="mx-2 my-2">
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
        <v-row no-gutters>
          <v-col cols="12" md="6">
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
              <p class="caption ecgray--text text--darken-2">{{$t('Origin')}}</p>
              <p>{{model.origin || '-'}}</p>
            </div>
            <template v-if="model.consumerAddress">
              <div class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('City')}}</p>
                <p>{{model.consumerAddress.city || '-'}}</p>
              </div>
              <div class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('Street')}}</p>
                <p>{{model.consumerAddress.street || '-'}}</p>
              </div>
              <div class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('Zip')}}</p>
                <p>{{model.consumerAddress.zip || '-'}}</p>
              </div>
            </template>
            <div class="info-block" v-if="model.billingDesktopRefNumber">
              <p class="caption ecgray--text text--darken-2">{{$t('BillingDesktopRefNumber')}}</p>
              <p>{{model.billingDesktopRefNumber}}</p>
            </div>
            <div class="info-block" v-if="model.consumerSecondPhone">
              <p class="caption ecgray--text text--darken-2">{{$t('SecondPhone')}}</p>
              <p>{{model.consumerSecondPhone}}</p>
            </div>
            <div class="info-block" v-if="model.consumerNote">
              <p class="caption ecgray--text text--darken-2">{{$t('Note')}}</p>
              <p>{{model.consumerNote}}</p>
            </div>
          </v-col>
          <v-col cols="12" md="6">
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('Active')}}</p>
              <p v-if="model.active" class="success--text">{{$t("Yes")}}</p>
              <p v-else class="error--text">{{$t("No")}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('NationalID')}}</p>
              <p>{{model.consumerNationalID || '-'}}</p>
            </div>
            <div class="info-block">
              <p class="caption ecgray--text text--darken-2">{{$t('ExternalReference')}}</p>
              <p>{{model.externalReference || '-'}}</p>
            </div>
            <div class="info-block" v-if="model.woocommerceID">
              <p class="caption ecgray--text text--darken-2">{{$t('WoocommerceID')}}</p>
              <p>{{model.woocommerceID || '-'}}</p>
            </div>
            <template v-if="model.consumerAddress">
              <div class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('House')}}</p>
                <p>{{model.consumerAddress.house || '-'}}</p>
              </div>
              <div class="info-block">
                <p class="caption ecgray--text text--darken-2">{{$t('Apartment')}}</p>
                <p>{{model.consumerAddress.apartment || '-'}}</p>
              </div>
            </template>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <bank-payment-details class="mx-2" card v-if="model.bankDetails" :model="model.bankDetails"></bank-payment-details>

    <v-card flat class="mx-2 my-2" :loading="tokens == null">
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
              <span v-bind:class="{'error--text': item.expired}" dir="ltr">
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
import { mapMutations } from 'vuex';

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    BankPaymentDetails: () => import("../../components/details/BankPaymentDetails"),
    CustomerDeleteDialog: () => import("../../components/customers/CustomerDeleteDialog"),
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
      tokens: null,
      deleteCustomerDialog: false,
    };
  },
  methods: {
    async restoreCustomer(){
      let result = await this.$api.consumers.restoreConsumer(this.$route.params.id);
      if (!this.$apiSuccess(result)) return;
      this.model.active = true;
      this.initMenu();
    },
    async showDeleteCustomerDialog(){
      this.deleteCustomerDialog = true;
    },
    async deleteCustomer(){
      if(this.model.active){
        let result = await this.$api.consumers.deleteConsumer(
          this.$route.params.id
        );
        if (!this.$apiSuccess(result)) return;
        this.refreshKeepAlive();
        return this.$router.push({ name: "Customers" });
      }
    },
    async deleteCardToken(tokenId) {
      let result = await this.$api.cardTokens.deleteCardToken(tokenId);

      if (!this.$apiSuccess(result)) return;
      this.tokens = (await this.$api.cardTokens.getCustomerCardTokens(this.$route.params.id)).data || [];
    },
    initMenu(){
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
              text: this.$t("CreateBillingDeal"),
              fn: () => {
                this.$router.push({
                  name: "CreateBillingDeal",
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
              text: this.model.active ? this.$t("DeleteCustomer") : this.$t("RestoreCustomer"),
              fn: () => this.model.active ? this.showDeleteCustomerDialog() : this.restoreCustomer(),
            }
          ],
          text: { translate: false, value: this.model.consumerName }
        }
      });
    },
    ...mapMutations({
      refreshKeepAlive: 'ui/refreshKeepAlive',
    }),
  },
  async mounted() {
    if (this.data) {
      this.model = this.data;
      return;
    }

    let customer = await this.$api.consumers.getConsumer(this.$route.params.id);

    if (!customer) {
      return this.$router.push({ name: "Customers" });
    }
    this.model = customer;

    this.tokens =
      (await this.$api.cardTokens.getCustomerCardTokens(this.$route.params.id))
        .data || [];

    this.initMenu();
  }
};
</script>

<style lang="scss" scoped>

</style>