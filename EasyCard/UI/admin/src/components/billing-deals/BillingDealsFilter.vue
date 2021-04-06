<template>
  <v-container fluid>
    <v-form ref="form" v-model="formIsValid">
      <v-row>
        <merchant-terminal-filter class="py-0" v-model="model"></merchant-terminal-filter>
        <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.billingDealID"
              :label="$t('BillingDealID')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.cardOwnerNationalID"
              :label="$t('CardOwnerNationalID')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.cardOwnerName"
              :label="$t('CardOwnerName')"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="4" class="py-0">
            <v-text-field
              v-model="model.consumerEmail"
              :label="$t('CustomerEmail')"
            ></v-text-field>
          </v-col>
          <!-- <v-col cols="12" md="4" class="py-0">
            <v-select
              :items="dictionaries.currencyEnum"
              item-text="description"
              item-value="code"
              v-model="model.currency"
              :label="$t('Currency')"
              clearable
            ></v-select>
          </v-col> -->
      </v-row>
      <v-row>
        <v-col cols="12" md="4" class="py-0">
          <v-select
            :items="dictionaries.quickDateFilterTypeEnum"
            item-text="description"
            item-value="code"
            v-model="model.quickDateFilter"
            :label="$t('QuickDate')"
            clearable
          ></v-select>
        </v-col>
        <v-col cols="12" md="4" class="py-0">
          <v-menu
            ref="dateFromMenu"
            v-model="dateFromMenu"
            :close-on-content-click="false"
            :return-value.sync="model.dateFrom"
            offset-y
            min-width="290px"
          >
            <template v-slot:activator="{ on }">
              <v-text-field v-model="model.dateFrom" :label="$t('DateFrom')" readonly v-on="on" clearable></v-text-field>
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
        <v-col cols="12" md="4" class="py-0">
          <v-menu
            ref="dateToMenu"
            v-model="dateToMenu"
            :close-on-content-click="false"
            :return-value.sync="model.dateTo"
            offset-y
            min-width="290px"
          >
            <template v-slot:activator="{ on }">
              <v-text-field v-model="model.dateTo" :label="$t('DateTo')" readonly v-on="on"  clearable></v-text-field>
            </template>
            <v-date-picker v-model="model.dateTo" no-title scrollable>
              <v-spacer></v-spacer>
              <v-btn text color="primary" @click="$refs.dateToMenu.save(model.dateTo)">{{$t("Ok")}}</v-btn>
            </v-date-picker>
          </v-menu>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn
            color="success"
            class="mr-4"
            @click="apply()"
            :disabled="!formIsValid"
          >{{$t('Apply')}}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  name: "MerchantsFilter",
  components: {
    MerchantTerminalFilter: () => import("../filtering/MerchantTerminalFilter")
  },
  data() {
    return {
      model: { ...this.filterData },
      dictionaries: {},
      vr: ValidationRules,
      formIsValid: true,
      dateFromMenu: false,
      dateToMenu: false,
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
  },
  props: {
    filterData: {
      type: Object
    }
  },
  methods: {
    apply() {
      if (!this.$refs.form.validate()) {
        return;
      }
      this.$emit("apply", {
        ...this.model,
        dateFrom: this.$formatDate(this.model.dateFrom),
        dateTo: this.$formatDate(this.model.dateTo)
      });
    }
  }
};
</script>