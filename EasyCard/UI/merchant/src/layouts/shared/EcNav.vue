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
          <v-list-item-title class="text-underline">
            <router-link class="ecnavLink--text" :to="{name: 'MyProfile'}">{{userName}}</router-link>
          </v-list-item-title>
          <!-- <v-list-item-subtitle>Subtext</v-list-item-subtitle> -->
        </v-list-item-content>
      </v-list-item>

      <v-divider></v-divider>

      <template v-for="item in items">
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
            v-for="(child, i) in item.children"
            :key="i"
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
import { mapState } from "vuex";
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
      items: [
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
              to: { name: "CreateInvoice" }
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
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "BillingDealsList",
              to: { name: "BillingDeals" }
            },
            {
              icon: "mdi-plus",
              text: "CreateBillingDeal",
              to: { name: "CreateBillingDeal" }
            }
          ]
        }
      ],
      userName: null
    };
  },
  async mounted() {
    this.userName = !!this.$oidc ? await this.$oidc.getUserDisplayName() : null;
    const self = this;
    let items = [];
    
    for(var i of this.items){
      if(!i.allowedFor){
        items.push(i);
      }else if(await this.$oidc.isInRole(i.allowedFor)){
        items.push(i);
      }
    }
    this.items = items;
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
  }
};
</script>