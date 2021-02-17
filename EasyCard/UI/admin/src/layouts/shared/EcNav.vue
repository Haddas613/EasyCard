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
      <v-list-item two-line>
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

export default {
  name: "EcNav",
  props: ["drawer"],
  components: { 
    LangSwitcher: () => import("../../components/LanguageSwitcher"), 
    Avatar: () => import("vue-avatar") 
  },
  data() {
    return {
      items: [
        {
          icon: "mdi-view-dashboard",
          text: "Dashboard",
          to: "/admin/dashboard"
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
              to: "/admin/transactions/list"
            }
          ]
        },
        {
          icon: "mdi-account",
          "icon-alt": "mdi-account-outline",
          text: "Merchants",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "MerchantsList",
              to: "/admin/merchants/list"
            },
            {
              icon: "mdi-account-network",
              text: "UsersList",
              to: "/admin/users/list"
            },
            {
              icon: "mdi-plus",
              text: "CreateMerchant",
              to: "/admin/merchants/create"
            }
          ]
        },
        {
          icon: 'mdi-tab',
          'icon-alt': 'mdi-tab',
          text: "Terminals",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "TerminalsList",
              to: "/admin/terminals/list"
            },
            {
              icon: "mdi-format-list-bulleted-type",
              text: "TerminalTemplates",
              to: "/admin/terminal-templates/list"
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
              to: "/admin/invoicing/list"
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
              to: "/admin/payment-requests/list"
            }
          ]
        },
        {
          icon: "mdi-rotate-right",
          "icon-alt": "mdi-rotate-right",
          text: "BillingDeals",
          expanded: false,
          children: [
            {
              icon: "mdi-format-list-bulleted",
              text: "BillingDealsList",
              to: "/admin/billing-deals/list"
            }
          ]
        },
        {
          icon: "mdi-console",
          "icon-alt": "mdi-console",
          text: "System",
          expanded: false,
          children: [
            {
              icon: "mdi-history",
              text: "SystemLogs",
              to: "/admin/system-logs/list"
            },
            {
              icon: "mdi-book-account",
              text: "Audits",
              to: "/admin/audit/list"
            }
          ]
        },
      ],
      userName: null
    };
  },
  async mounted() {
    //TODO: check profile roles
    this.userName = !!this.$oidc ? (await this.$oidc.getUserDisplayName()) : null;
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
    }
  }
};
</script>