<template>
  <div>
    <v-btn color="primary px-0" small text @click="filterDialog = true;" v-if="dictionariesRaw.quickDateFilterTypeEnum">
      {{title}}
    </v-btn>
    <ec-dialog :dialog.sync="filterDialog">
      <template v-slot:title>
        {{$t('SelectDate')}}
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
              <v-col cols="12" md="6" class="py-0 px-6">
                <v-menu
                  ref="dateFromMenu"
                  v-model="dateFromMenu"
                  :close-on-content-click="false"
                  :return-value.sync="model.dateFrom"
                  offset-y
                  min-width="290px"
                >
                  <template v-slot:activator="{ on }">
                    <v-text-field
                      v-model="model.dateFrom"
                      :label="$t('DateFrom')"
                      readonly
                      :rules="[vr.primitives.requiredDependsOnFalsy(model.dateTo)]"
                      outlined
                      v-on="on"
                    ></v-text-field>
                  </template>
                  <v-date-picker v-model="model.dateFrom" no-title scrollable>
                    <v-spacer></v-spacer>
                    <v-btn
                      text
                      color="primary"
                      @click="$refs.dateFromMenu.save(model.dateFrom)"
                    >{{$t("Ok")}}</v-btn>
                  </v-date-picker>
                </v-menu>
              </v-col>
              <v-col cols="12" md="6" class="py-0 px-6">
                <v-menu
                  ref="dateToMenu"
                  v-model="dateToMenu"
                  :close-on-content-click="false"
                  :return-value.sync="model.dateTo"
                  offset-y
                  min-width="290px"
                >
                  <template v-slot:activator="{ on }">
                    <v-text-field
                      v-model="model.dateTo"
                      :label="$t('DateTo')"
                      readonly
                      :rules="[vr.primitives.requiredDependsOnFalsy(model.dateFrom)]"
                      outlined
                      v-on="on"
                    ></v-text-field>
                  </template>
                  <v-date-picker v-model="model.dateTo" no-title scrollable>
                    <v-spacer></v-spacer>
                    <v-btn
                      text
                      color="primary"
                      @click="$refs.dateToMenu.save(model.dateTo)"
                    >{{$t("Ok")}}</v-btn>
                  </v-date-picker>
                </v-menu>
              </v-col>
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
  },
  data() {
    return {
      filterDialog: false,
      dictionaries: {},
      dictionariesRaw: {},
      model: {
        ...this.storeDateFilter
      },
      dateFromMenu: null,
      dateToMenu: null,
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
      this.$store.commit('ui/setDashboardDateFilterAlt',  this.model);
      this.filterDialog = false;
    }
  },
  computed: {
    ...mapState({
      storeDateFilter: state => state.ui.dashboardDateFilterAlt,
    }),
    title: function(){
      if(this.storeDateFilter.dateFrom || this.storeDateFilter.dateTo){
        let df = this.storeDateFilter.dateFrom;
        let dt = this.storeDateFilter.dateTo;
        return `${df ? moment(df).format("MM/DD") : "~"} - ${dt ? moment(dt).format("MM/DD") : "~"}`;
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