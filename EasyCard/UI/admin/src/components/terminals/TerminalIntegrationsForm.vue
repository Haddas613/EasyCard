<template>
  <div>
    <ec-dialog :dialog.sync="integrationDialog">
      <template v-slot:title>{{$t('AddIntegration')}}</template>
      <template>
        <v-form class="pt-2" ref="form" v-model="valid">
          <v-row>
            <v-col cols="12" class="py-0">
              <v-select
                :items="integrationTypes"
                item-text="name"
                item-value="value"
                v-model="selectedIntegrationType"
                :label="$t('IntegrationType')"
                :rules="[vr.primitives.required]"
                outlined
              ></v-select>
            </v-col>
            <v-col cols="12" class="py-0" v-if="selectedIntegrationType">
              <v-select
                :items="integrations[selectedIntegrationType]"
                item-text="name"
                item-value="externalSystemID"
                v-model="selectedIntegrationID"
                :label="$t('IntegrationName')"
                :rules="[vr.primitives.required]"
                outlined
              ></v-select>
            </v-col>
          </v-row>
          <div class="d-flex px-2 pt-4 justify-end">
            <v-btn
              color="primary"
              class="white--text"
              :block="$vuetify.breakpoint.smAndDown"
              @click="addIntegration()"
            >{{$t("OK")}}</v-btn>
          </div>
        </v-form>
      </template>
    </ec-dialog>
    <div class="d-flex justify-end pb-2">
      <v-btn color="success" @click="showIntegrationDialog()" :disabled="!integrationTypes || (integrationTypes.length == model.integrations.length)">
        {{$t("AddIntegration")}}
      </v-btn>
    </div>
    <div v-if="model.integrations">
      <v-card
        v-for="int in model.integrations"
        :key="int.externalSystemID"
        no-gutters
        class="mb-4 mx-4"
        v-bind:class="{'invalid-integration': !isTemplate && !int.valid}"
      >
        <template v-if="int.externalSystem">
          <v-card-title class="subtitle-2" >
            <v-row no-gutters>
              <v-col cols="9" class="pt-2">
                {{int.externalSystem.name}}
              </v-col>
              <v-col cols="3" class="d-flex justify-end">
                <v-btn x-small fab outlined color="red" @click="deleteIntegration(int.externalSystemID)">
                  <v-icon>mdi-delete</v-icon>
                </v-btn> 
              </v-col>
            </v-row>
          </v-card-title>
          <v-divider></v-divider>
          <v-card-text>
            <component
              v-bind:is="getIntegrationComponentName(int.externalSystem.key)"
              v-bind="{data: int, terminalId: terminal[idKey], apiName: apiName}"
              v-if="$options.components[getIntegrationComponentName(int.externalSystem.key)]"
              v-on:update="refreshIntegration($event)"
            ></component>
            <p v-else>
              {{$t("NotSupportedContactAdministration")}}
            </p>
          </v-card-text>
        </template>
      </v-card>
    </div>
  </div>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
export default {
  components: {
    EcDialog: () => import("../ec/EcDialog")
  },
  props: {
    terminal: {
      type: Object,
      required: true
    },
    isTemplate: {
      type: Boolean,
      default: false,
      required: false
    }
  },
  data() {
    return {
      model: { ...this.terminal },
      integrations: null,
      integrationTypes: null,
      selectedIntegrationType: null,
      selectedIntegrationID: null,
      integrationDialog: false,
      valid: false,
      vr: ValidationRules,
      idKey: 'terminalID',
      //api key for integrations to use ($api['terminals'] for example)
      apiName: this.isTemplate ? 'terminalTemplates' : 'terminals',
      loading: false,
    };
  },
  async mounted() {
    let integrations = await this.$api.terminals.getAvailableIntegrations({
      //showForTemplatesOnly: this.isTemplate
    });
    this.integrationTypes = Object.keys(integrations).map(e => {
      return {
        name: this.$t(e),
        value: e,
        disabled: this.lodash.some(this.model.integrations, i => i.externalSystem && i.externalSystem.type == e)
      }
    });
    this.integrations = integrations;
    if(this.apiName == 'terminalTemplates'){
      this.idKey = 'terminalTemplateID';
    }
  },
  methods: {
    getIntegrationComponentName(key) {
      return `${key}-settings-form`;
    },
    showIntegrationDialog() {
      this.selectedIntegrationType = null;
      this.integrationDialog = true;
    },
    async addIntegration(){
      if(!this.$refs.form.validate() || this.loading){ return ;}
      this.loading = true;
      try{
        let type = this.lodash.find(this.integrationTypes, t => t.name == this.selectedIntegrationType);
        type.disabled = true;

        let integration = this.lodash.find(this.integrations[type.name], i => i.externalSystemID == this.selectedIntegrationID);
        let opResult = await this.$api[this.apiName].saveExternalSystem(this.terminal[this.idKey], integration);

        if(!this.isTemplate && opResult.additionalData){
          integration.valid = opResult.additionalData.valid;
        }

        integration = this.mapIntegration(integration);
        this.model.integrations.push(integration);

        this.selectedIntegrationType = null;
        this.integrationDialog = false;
      }finally{
        this.loading = false;
      }
    },

    mapIntegration(int){
      return {
        //for compatibility
        externalSystem: {
          externalSystemID: int.externalSystemID,
          name: int.name,
          key: int.key,
          type: int.type
        },
        externalSystemID: int.externalSystemID,
        settings: int.settings,
        valid: int.valid,
      }
    },
    async deleteIntegration(integrationID){
      await this.$api[this.apiName].deleteExternalSystem(this.terminal[this.idKey], integrationID);
      let idx = this.lodash.findIndex(this.model.integrations, i => i.externalSystemID == integrationID);

      let type = this.lodash.find(this.integrationTypes, t => t.name == this.model.integrations[idx].externalSystem.type);
      type.disabled = false;
      this.model.integrations.splice(idx, 1);
    },
    refreshIntegration(data){
      let idx = this.lodash.findIndex(this.model.integrations, i => i.externalSystemID == data.externalSystemID);
      if (idx > -1){
        this.$set(this.model.integrations, idx, data);
      }
    }
  }
};
</script>

<style lang="scss" scoped>
.invalid-integration{
  border: 1px solid var(--v-error-base);
}
</style>