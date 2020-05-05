<template>
  <v-app id="inspire" v-bind:dir="$vuetify.rtl ? 'rtl' : 'ltr'">
    {{drawer}}
    <main-app-bar :drawer.sync="drawer"/>

    <main-nav :items="items" :drawer.sync="drawer"/>

    <main-content />

    <main-footer />
  </v-app>
</template>

<script>
  export default {
    name: 'MainIndex',

    components: {
      MainAppBar: () => import('./AppBar'),
      MainNav: () => import('./Nav'),
      MainContent: () => import('./Content'),
      MainFooter: () => import('./Footer'),
    },

    methods: {
      changeRTL (to) {
        this.$vuetify.rtl = to;
      },
    },

    data: () => ({
      dialog: false,
      drawer: false,
      items: [
        { icon: 'mdi-contacts', text: 'Contacts' },
        { icon: 'mdi-history', text: 'Frequently contacted' },
        { icon: 'mdi-content-copy', text: 'Duplicates' },
        {
          icon: 'mdi-chevron-up',
          'icon-alt': 'mdi-chevron-down',
          text: 'Labels',
          model: true,
          children: [
            { icon: 'mdi-plus', text: 'Create label' },
          ],
        },
        {
          icon: 'mdi-chevron-up',
          'icon-alt': 'mdi-chevron-down',
          text: 'More',
          model: false,
          children: [
            { text: 'Import' },
            { text: 'Export' },
            { text: 'Print' },
            { text: 'Undo changes' },
            { text: 'Other contacts' },
          ],
        },
        { icon: 'mdi-settings', text: 'Settings' },
        { icon: 'mdi-message', text: 'Send feedback' },
        { icon: 'mdi-help-circle', text: 'Help' },
        { icon: 'mdi-cellphone-link', text: 'App downloads' },
        { icon: 'mdi-keyboard', text: 'Go to the old version' },
      ],
    }),
  }
</script>
