<template>
  <v-navigation-drawer
    v-model="drawerObj"
    :clipped="$vuetify.breakpoint.lgAndUp"
    app
    :right="isRtl"
    :color="'ecbg lighten-4'"
    class="will-change-inherit"
  >
    <v-list class="py-0">
      <v-list-item two-line class="py-4">
        <v-list-item-avatar>
          <avatar v-if="userName" :username="userName" :rounded="true"></avatar>
        </v-list-item-avatar>

        <v-list-item-content>
          <v-list-item-title class="text-underline mt-4">
            <router-link class="ecnavLink--text" :to="{name: 'MyProfile'}">{{userName}}</router-link>
            <p>
              <v-icon small>mdi-account-key</v-icon>
              <small class="px-1">{{idlingTimeLeft}}</small>
            </p>
          </v-list-item-title>
          <!-- <v-list-item-subtitle>Subtext</v-list-item-subtitle> -->
        </v-list-item-content>
      </v-list-item>

      <v-divider></v-divider>

      <template v-for="(item, i) in items">
        <v-list-group
          v-if="item.children"
          :key="item.text"
          v-model="item.expanded"
          :prepend-icon="item.expanded ? item.icon : item['icon-alt']"
          :color="'ecnavLink'"
          append-icon
        >
          <template v-slot:activator>
            <v-list-item-content>
              <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
            </v-list-item-content>
          </template>
          <v-list-item
            v-for="(child, j) in item.children"
            :key="i + '_' + j"
            link
            :to="child.to"
            class="px-6"
          >
            <v-list-item-action v-if="child.icon">
              <v-icon>{{ child.icon }}</v-icon>
            </v-list-item-action>
            <v-list-item-content>
              <v-list-item-title>{{ $t(child.text) }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list-group>
        <v-divider v-else-if="item.divider" :key="item.dividerArea"></v-divider>
        <v-list-item v-else-if="item.fn" :key="item.text" @click="item.fn()">
          <v-list-item-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item v-else :key="item.text" link :to="item.to">
          <v-list-item-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title>{{ $t(item.text) }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </template>
    </v-list>
  </v-navigation-drawer>
</template>
<script>
import { mapState, mapGetters } from "vuex";
import Avatar from "vue-avatar";
import appConstants from "../../helpers/app-constants";

export default {
  components: {
    Avatar
  },
  name: "EcNav",
  props: ["drawer"],
  data() {
    return {
      items: null,
      userName: null,
      allItems: () => [
        {
          icon: "mdi-view-dashboard",
          text: "Dashboard",
          to: { name: "Dashboard" },
          allowedFor: [appConstants.users.roles.manager, appConstants.users.roles.billingAdmin]
        },
        {
          icon: "mdi-cash-minus",
          "icon-alt": "mdi-cash-plus",
          text: "Transactions",
          expanded: false,
          children: [
            {
              icon: "mdi-cash-multiple",
              text: "TransactionsList",
              to: { name: "Transactions" }
            },
            {
              icon: "mdi-chevron-down-box-outline",
              to: { name: "Charge" },
              text: "Charge"
            },
            // {
            //   icon: "mdi-credit-card-off",
            //   to: { name: "NonCardCharge" },
            //   text: "NonCardCharge"
            // },
            {
              icon: "mdi-chevron-up-box-outline",
              to: { name: "Refund" },
              text: "Refund",
              allowedFor: [appConstants.users.roles.manager, appConstants.users.roles.billingAdmin]
            }
          ]
        },
        {
          icon: "mdi-account",
          "icon-alt": "mdi-account-outline",
          text: "Customers",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "CustomersList",
              to: { name: "Customers" }
            },
            {
              icon: "mdi-plus",
              text: "CreateCustomer",
              to: { name: "CreateCustomer" }
            }
          ]
        },
        {
          icon: "mdi-basket",
          "icon-alt": "mdi-basket-outline",
          text: "Items",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "ItemsList",
              to: { name: "Items" }
            },
            {
              icon: "mdi-plus",
              text: "CreateItem",
              to: { name: "CreateItem" }
            }
          ]
        },
        {
          icon: "mdi-file-document",
          "icon-alt": "mdi-file-document-outline",
          text: "Invoicing",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "InvoicesList",
              to: { name: "Invoices" }
            },
            {
              icon: "mdi-plus",
              text: "CreateInvoice",
              to: { name: "CreateInvoice" },
              requiredIntegration: appConstants.terminal.integrations.invoicing,
            }
          ]
        },
        {
          icon: "mdi-account-cash",
          "icon-alt": "mdi-account-cash-outline",
          text: "PaymentRequests",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "PaymentRequestsList",
              to: { name: "PaymentRequests" }
            },
            {
              icon: "mdi-plus",
              text: "CreatePaymentRequest",
              to: { name: "CreatePaymentRequest" }
            }
          ]
        },
        {
          icon: "mdi-rotate-right",
          "icon-alt": "mdi-rotate-right",
          text: "BillingDeals",
          allowedFor: [appConstants.users.roles.manager, appConstants.users.roles.billingAdmin],
          requiredFeature: appConstants.terminal.features.Billing,
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "BillingDealsList",
              to: { name: "BillingDeals" }
            },
            {
              icon: "mdi-chart-timeline-variant",
              text: "FutureBillingDeals",
              to: { name: "FutureBillingDeals" }
            },
            {
              icon: "mdi-plus",
              text: "CreateBillingDeal",
              to: { name: "CreateBillingDeal" }
            }
          ]
        }
      ],
    };
  },
  async mounted() {
    this.userName = !!this.$oidc ? await this.$oidc.getUserDisplayName() : null;
    await this.initMenuItems();
  },
  methods: {
    async initMenuItems() {
      let items = await this.filterMenuItems(this.allItems());
      for(var i of items){
        if(i.children && i.children.length > 0){
          i.children = await this.filterMenuItems(i.children);
        }
      }
      this.items = items;
    },
    async filterMenuItems(items){
      let result = [];
      const _filterMenuItem = async (i) => {
        if(i.allowedFor && !(await this.$oidc.isInRole(i.allowedFor))){
          return false;
        }
        if(i.requiredFeature && !(this.$featureEnabled(this.terminalStore, i.requiredFeature))){
          return false;
        }
        if(i.requiredIntegration && !(this.$integrationAvailable(this.terminalStore, i.requiredIntegration))){
          return false;
        }
        return true;
      };
      for(var i of items){
        if(await _filterMenuItem(i)){
          result.push(i);
        }
      }
      return result;
    }
  },
  computed: {
    drawerObj: {
      get: function() {
        return this.drawer;
      },
      set: function(nv) {
        this.$emit("update:drawer", nv);
      }
    },
    isRtl: {
      cache: false,
      get: function() {
        return this.$vuetify.rtl === true;
      }
    },
    ...mapState({
      terminalStore: state => state.settings.terminal,
      idlingStore: state => state.idling,
    }),
    ...mapGetters({
      idlingTimeLeft: 'idling/idlingTimeLeft',
    }),
  },
  watch: {
    'terminalStore': async function(newValue, oldValue) {
      await this.initMenuItems();
    }
  },
};
</script>