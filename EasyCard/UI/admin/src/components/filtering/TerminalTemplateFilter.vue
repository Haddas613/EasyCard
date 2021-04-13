<template>
    <v-col cols="12" md="3" sm="6">
      <v-autocomplete
        :items="templates"
        item-text="label"
        item-value="terminalTemplateID"
        v-model="data.terminalTemplateID"
        :search-input.sync="searchTemplates"
        :label="$t('TerminalTemplate')"
        hide-details
        clearable
      ></v-autocomplete>
    </v-col>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  model: {
    prop: "data",
    event: "change"
  },
  props: {
    data: {
      type: Object,
      default: null,
      required: true
    }
  },
  data() {
    return {
      vr: ValidationRules,
      searchTemplates: null,
      searchTemplatesTimeout: null,
      selectedTemplate: null,
      templates: []
    };
  },
  watch: {
    async searchTemplates(val) {
      //Vuetify search-input event is triggered when terminal is selected producing redundant event, it should be ignored
      if (this.selectedTemplate && val == this.selectedTemplate.label) {
        return;
      }
      if (this.searchTemplatesTimeout) {
        clearTimeout(this.searchTemplatesTimeout);
      }

      this.searchTemplatesTimeout = setTimeout(
        () => this.getTemplates(!val || val.length < 3 ? null : val),
        1500
      );
    }
  },
  methods: {
    async getTemplates(search, terminalTemplateID = null) {
      let response = await this.$api.terminalTemplates.getTerminalTemplatesRaw({
        label: search,
        take: 10,
        terminalTemplateID: terminalTemplateID
      });
      this.templates = response.data;
      if (this.templates.length === 1) {
        this.selectedTemplate = this.templates[0];
      }
    }
  },
  async mounted() {
    this.getTemplates(null, this.data.terminalTemplateID);
  }
};
</script>