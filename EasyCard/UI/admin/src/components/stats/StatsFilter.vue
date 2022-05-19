<template>
  <div>
    <v-btn color="primary px-0" small text @click="filterDialog = true;" v-if="dictionariesRaw.quickDateFilterTypeEnum">
      {{title}}
    </v-btn>
    <ec-dialog :dialog.sync="filterDialog">
      <template v-slot:title>
        {{$t('Filter')}}
      </template>
      <template v-slot:right>
        <v-btn color="primary" @click="apply()">{{$t('Apply')}}</v-btn>
      </template>
      <template>
        <v-form ref="form" v-model="formIsValid">
          <v-row>
            <v-col cols="12" class="py-0 px-6">
              <v-switch v-model="model.customDate" :label="$t('Custom')"></v-switch>
            </v-col>
            <template v-if="model.customDate">
              <date-from-to-filter class="px-3 px-6" v-model="model"></date-from-to-filter>
            </template>
            <v-col cols="12" class="py-0" v-else>
              <ec-radio-group
                :data="dictionaries.quickDateFilterTypeEnum"
                valuekey="code"
                :model.sync="model.quickDateType"
              >
                <template v-slot="{ item }">{{item.description}}</template>
              </ec-radio-group>
            </v-col>
          </v-row>
        </v-form>
      </template>
    </ec-dialog>
  </div>
</template>

<script>
import { mapState } from "vuex";
import ValidationRules from "../../helpers/validation-rules";
import moment from "moment";

export default {
  components: {
    EcDialog: () => import("../ec/EcDialog"),
    EcRadioGroup: () => import("../inputs/EcRadioGroup"),
    DateFromToFilter: () => import("../filtering/DateFromToFilter"),
  },
  data() {
    return {
      filterDialog: false,
      dictionaries: {},
      dictionariesRaw: {},
      model: {
        ...this.storeDateFilter
      },
      formIsValid: true,
      vr: ValidationRules
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.dictionariesRaw = await this.$api.dictionaries.$getTransactionDictionaries();
    this.$set(this, 'model', JSON.parse(JSON.stringify(this.storeDateFilter)));
  },
  methods: {
    apply() {
      if(!this.$refs.form.validate()){
        return;
      }
      if(!this.model.customDate){
        this.model.dateFrom =  this.model.dateTo = null;
      }
      this.$store.commit('ui/setDashboardDateFilter',  this.model);
      this.filterDialog = false;
    }
  },
  computed: {
    ...mapState({
      storeDateFilter: state => state.ui.dashboardDateFilter,
    }),
    title: function(){
      if(this.storeDateFilter.dateFrom || this.storeDateFilter.dateTo){
        let df = this.storeDateFilter.dateFrom;
        let dt = this.storeDateFilter.dateTo;
        return `${df ? moment(df).format("DD/MM") : "~"} - ${dt ? moment(dt).format("DD/MM") : "~"}`;
      }

      return this.dictionariesRaw.quickDateFilterTypeEnum[this.storeDateFilter.quickDateType] 
        ? this.dictionariesRaw.quickDateFilterTypeEnum[this.storeDateFilter.quickDateType] : "-";
    }
  },
  watch: {
    storeDateFilter(newValue, oldValue) {
      this.model = JSON.parse(JSON.stringify(newValue));
    }
  },
};
</script>