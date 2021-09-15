<template>
  <v-flex>
    <ec-list :items="data" v-if="data" dashed>
      <template v-slot:prepend="{ item }" v-if="selectable">
         <v-checkbox
            v-model="item.selected"
            @change="itemSelected(item)"
         ></v-checkbox>
      </template>
      <template v-slot:prepend v-else>
        <v-icon>mdi-credit-card-outline</v-icon>
      </template>

      <template v-slot:left="{ item }">
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="pt-1 caption ecgray--text"
        >
          <p class="my-0">{{item.$date | ecdate('DD/MM/YYYY HH:mm')}}</p>
          <p class="my-0 primary--text">
            {{ item.solek }}
          </p>
        </v-col>
        <v-col cols="12" md="6" lg="6" class="d-flex align-center secondary--text caption">
          <span v-if="$vuetify.breakpoint.smAndDown">{{ item.shvaDealID | rlength(5)}}</span>
          <span v-else>{{ item.shvaDealID }}</span>
        </v-col>
      </template>

      <template v-slot:right="{ item }">
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end body-2"
        >
          {{ item.consumerName }}
        </v-col>
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end font-weight-bold button"
          v-bind:class="{'red--text': item.$specialTransmissionType == 'refund'}"
        >{{item.totalAmount | currency(item.$currency)}}</v-col>
      </template>

      <template v-slot:append="{ item }">
        <v-btn icon :to="{ name: 'Transaction', params: { id: item.$paymentTransactionID } }">
          <re-icon>mdi-chevron-right</re-icon>
        </v-btn>
      </template>
    </ec-list>
    <p
      class="ecgray--text text-center"
      v-if="data && data.length === 0"
    >{{$t("NothingToShow")}}</p>
  </v-flex>
</template>

<script>
import moment from "moment";

export default {
  components: {
    EcList: () => import("../../components/ec/EcList"),
    ReIcon: () => import("../../components/misc/ResponsiveIcon")
  },
  props: {
    transmissions: {
        required: true,
        default: () => []
    },
    selectable: {
      type: Boolean,
      required: false,
      default: false
    },
    selectLimit: {
      type: Number,
      default: 0
    }
  },
  data() {
    return {
      data: this.transmissions,
      customerInfo: null,
      moment: moment,
      transactionSlipDialog: false,
      loadingTransmission: false
    };
  }
};
</script>