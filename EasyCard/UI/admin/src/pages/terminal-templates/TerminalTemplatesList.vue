<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>
      <v-flex class="d-flex justify-end">
        <v-btn color="success" @click="showCreateTerminalTemplateDialog = true;">
          {{$t("CreateNew")}}
        </v-btn>
      </v-flex>
      <create-terminal-template-dialog :show.sync="showCreateTerminalTemplateDialog" v-on:ok="getDataFromApi()"></create-terminal-template-dialog>
    </v-card-title>
    <v-expansion-panels :flat="true">
      <v-expansion-panel>
        <v-expansion-panel-header>{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <terminal-templates-filter  :filter-data="terminalTemplatesFilter" v-on:apply="applyFilter($event)"></terminal-templates-filter>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-divider></v-divider>
    <div>
      <v-data-table
        :headers="headers"
        :items="terminalTemplates"
        :options.sync="options"
        :server-items-length="totalAmount"
        :loading="loading"
        :header-props="{ sortIcon: null }"
        class="elevation-1"
      >
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="secondary" outlined small link :to="{name: 'EditTerminalTemplate', params: {id: item.terminalTemplateID}}">
            <v-icon small>mdi-pencil</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="error" outlined small v-if="isBillingAdmin && item.active" @click="disapprove(item)">
            <v-icon small>mdi-cancel</v-icon>
          </v-btn>
          <v-btn class="mx-1" color="success" outlined small v-if="isBillingAdmin && !item.active" @click="approve(item)">
            <v-icon small>mdi-chevron-down-circle</v-icon>
          </v-btn>
          <!-- <v-icon small @click="deleteItem(item)">mdi-delete</v-icon> -->
        </template>
      </v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  components: {
    CreateTerminalTemplateDialog: () => import("../../components/terminal-templates/CreateTerminalTemplateDialog"),
    TerminalTemplatesFilter: () => import("../../components/terminals/TerminalTemplatesFilter")
  },
  data() {
    return {
      totalAmount: 0,
      terminalTemplates: [],
      loading: true,
      options: {},
      pagination: {},
      headers: [],
      terminalTemplatesFilter: {},
      showCreateTerminalTemplateDialog: false,
      isBillingAdmin: false,
    };
  },
  watch: {
    options: {
      handler: async function() {
        await this.getDataFromApi();
      },
      deep: true
    }
  },
  async mounted () {
    this.isBillingAdmin = await this.$oidc.isBillingAdmin();
  },
  methods: {
    async getDataFromApi() {
      this.loading = true;
      let data = await this.$api.terminalTemplates.get({
        ...this.terminalTemplatesFilter,
        ...this.options
      });
      this.terminalTemplates = data.data;
      this.totalAmount = data.numberOfRecords;
      this.loading = false;

      if (!this.headers || this.headers.length === 0) {
        this.headers = [...data.headers, { value: "actions", text: this.$t("Actions"), sortable: false  }];
      }
    },
    async applyFilter(filter) {
      this.options.page = 1;
      this.terminalTemplatesFilter = filter;
      await this.getDataFromApi();
    },
    async approve(terminalTemplate){
      await this.$api.terminalTemplates.approve(terminalTemplate.terminalTemplateID);
      terminalTemplate.active = true;
    },
    async disapprove(terminalTemplate){
      await this.$api.terminalTemplates.disapprove(terminalTemplate.terminalTemplateID);
      terminalTemplate.active = false;
    }
  }
};
</script>