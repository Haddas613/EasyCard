<template>
  <v-flex class="d-flex flex-column">
    <ec-dialog-invoker v-on:click="invoiceTypeDialog = true" class="py-2">
      <template v-slot:left>
        <div class="font-weight-medium">{{$t("InvoiceType")}}</div>
      </template>
      <template v-slot:right>
        <div>{{model.invoiceType ? model.invoiceType.description : $t('PleaseSelect')}}</div>
      </template>
      <template v-slot:append>
        <re-icon>mdi-chevron-right</re-icon>
      </template>
    </ec-dialog-invoker>
    <ec-dialog :dialog.sync="invoiceTypeDialog">
      <template v-slot:title>{{$t('InvoiceType')}}</template>
      <template>
        <ec-radio-group
          :data="dictionaries.invoiceTypeEnum"
          labelkey="description"
          valuekey="code"
          return-object
          :model.sync="model.invoiceType"
          @change="invoiceTypeDialog = false;"
        ></ec-radio-group>
      </template>
    </ec-dialog>
    <!-- <v-text-field
      v-model="model.invoiceNumber"
      :label="$t('InvoiceNumber')"
      :rules="[vr.primitives.required, vr.primitives.maxLength(50)]"
      outlined
      @keydown.native.space.prevent
      required
    ></v-text-field> -->
    <v-text-field
      v-model="model.invoiceSubject"
      :label="$t('InvoiceSubject')"
      :rules="[vr.primitives.required, vr.primitives.maxLength(255)]"
      required
      outlined
    ></v-text-field>
    <v-textarea
      v-model="model.sendCCToRaw"
      outlined
      :hint="$t('SeparateEmailsWithCommas')"
      persistent-hint
      rows="3"
    >
      <template v-slot:label>
        <div>{{$t('AdditionalEmailToCC')}}</div>
      </template>
    </v-textarea>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";
import { mapState } from "vuex";

export default {
  components: {
    ReIcon: () => import("../../components/misc/ResponsiveIcon"),
    EcDialog: () => import("../../components/ec/EcDialog"),
    EcDialogInvoker: () => import("../../components/ec/EcDialogInvoker"),
    EcRadioGroup: () => import("../../components/inputs/EcRadioGroup")
  },
  props: {
    data: {
      default: null,
      required: false
    }
  },
  data() {
    return {
      dictionaries: {},
      model: {
        ...this.data
      },
      vr: ValidationRules,
      invoiceTypeDialog: false
    };
  },
  computed: {
    ...mapState({
      currencyStore: state => state.settings.currency,
      terminalStore: state => state.settings.terminal
    })
  },
  async mounted() { 
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();
    if(!this.model.invoiceSubject){
      this.model.invoiceSubject = this.terminalStore.invoiceSettings.defaultInvoiceSubject;
    }
    
    if(!this.model.invoiceType){
      this.$set(this.model, 'invoiceType', this.dictionaries.invoiceTypeEnum.find(i => i.code == this.terminalStore.invoiceSettings.defaultInvoiceType));
    }else if(typeof(this.model.invoiceType) === "string"){
      this.$set(this.model, 'invoiceType', this.dictionaries.invoiceTypeEnum.find(i => i.code == this.model.invoiceType));
    }
  },
  methods: {
    getData() {
      if(!this.model.invoiceType){
        this.invoiceTypeDialog = true;
        return false;
      }

      let result = { ...this.model };
      if (result.sendCCToRaw) {
        result.sendCCTo = [];
        let split = result.sendCCToRaw.split(",");
        for (var s of split) {
          let trimmed = s.trim();
          if (trimmed && this.vr.primitives.email(trimmed) === true) {
            result.sendCCTo.push(trimmed);
          }
        }
        delete result.sendCCToRaw;
      }
      result.invoiceType = result.invoiceType.code;
      
      return result;
    }
  }
};
</script>