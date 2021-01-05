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
        <v-expansion-panel-header class="primary white--text">{{$t('Filters')}}</v-expansion-panel-header>
        <v-expansion-panel-content>
          <div class="pt-4 pb-2">filter: work in progress</div>
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
        class="elevation-1"
      >
        <template v-slot:item.actions="{ item }">
          <v-btn class="mx-1" color="secondary" outlined x-small link :to="{name: 'EditTerminalTemplate', params: {id: item.terminalTemplateID}}">
            <v-icon small>mdi-pencil</v-icon>
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
      showCreateTerminalTemplateDialog: false
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
        this.headers = [...data.headers, { value: "actions", text: this.$t("Actions") }];
      }
    },
    //TODO
    async applyFilter(filter) {
      this.terminalTemplatesFilter = filter;
      await this.getDataFromApi();
    }
  }
};
</script>