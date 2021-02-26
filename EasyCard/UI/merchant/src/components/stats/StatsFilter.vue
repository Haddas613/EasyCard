<template>
  <div>
    <v-btn color="primary" small text @click="filterDialog = true;" v-if="dictionariesRaw.quickDateFilterTypeEnum">
        {{dictionariesRaw.quickDateFilterTypeEnum[storeDateFilter] ? dictionariesRaw.quickDateFilterTypeEnum[storeDateFilter] : "-"}}
    </v-btn>
    <ec-dialog :dialog.sync="filterDialog">
      <template v-slot:title>{{$t('SelectDate')}}</template>
      <template>
        <ec-radio-group
          :data="dictionaries.quickDateFilterTypeEnum"
          valuekey="code"
          :model.sync="selectedDate"
        >
          <template v-slot="{ item }">{{item.description}}</template>
        </ec-radio-group>
      </template>
    </ec-dialog>
  </div>
</template>

<script>
import { mapState } from "vuex";

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
    };
  },
  async mounted() {
    this.dictionaries = await this.$api.dictionaries.getTransactionDictionaries();
    this.dictionariesRaw = await this.$api.dictionaries.$getTransactionDictionaries();
  },
  computed: {
    ...mapState({
      storeDateFilter: state => state.ui.dashboardDateFilter,
    }),
    selectedDate: {
      get: function() {
        return this.storeDateFilter;
      },
      set: function(nv) {
        this.$store.commit('ui/setDashboardDateFilter', nv);
        this.filterDialog = false;
      }
    },
  },
};
</script>

<style lang="scss" scoped>
</style>