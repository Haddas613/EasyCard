<template>
  <v-flex>
    <ec-list :items="data">
      <template v-slot:prepend="{ item }" v-if="selectable">
        <v-checkbox v-model="item.selected" :disabled="item.$status == 'sending'"></v-checkbox>
      </template>
      <template v-slot:left="{ item }">
        <v-col cols="12" md="6" lg="6" class="pt-1 caption ecgray--text">
          {{item.$invoiceDate | ecdate('DD/MM/YYYY')}}
          <v-chip color="primary" v-if="item.invoiceNumber" x-small>{{item.invoiceNumber}}</v-chip>
        </v-col>
        <v-col cols="12" md="6" lg="6">{{item.cardOwnerName || '-'}}</v-col>
      </template>

      <template v-slot:right="{ item }">
        <v-col cols="12" md="6" lg="6" class="text-end body-2">
          <v-btn
            outlined
            color="success"
            x-small
            v-if="item.$status == 'sent'"
            :title="$t('ClickToDownload')"
            @click="downloadInvoicePDF(item.$invoiceID)"
          >
            {{$t(item.status)}}
            <v-icon right color="red" size="1rem">mdi-file-pdf-outline</v-icon>
          </v-btn>
          <span v-bind:class="statusColors[item.$status]" v-else>{{$t(item.status || 'None')}}</span>
        </v-col>
        <v-col
          cols="12"
          md="6"
          lg="6"
          class="text-end font-weight-bold button"
        >{{item.invoiceAmount | currency(item.$currency)}}</v-col>
      </template>

      <template v-slot:append="{ item }">
        <v-btn icon :to="{ name: 'Invoice', params: { id: item.$invoiceID } }">
          <re-icon>mdi-chevron-right</re-icon>
        </v-btn>
      </template>
    </ec-list>
    <p class="ecgray--text text-center" v-if="data && data.length === 0">{{$t("NothingToShow")}}</p>
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
    invoices: {
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
      data: this.invoices,
      statusColors: {
        pending: "gray--text",
        none: "",
        sent: "success--text",
        sending: "primary--text",
        sendingFailed: "error--text"
      },
      customerInfo: null,
      moment: moment
    };
  },
  methods: {
    itemSelected(item) {
      if (!this.selectLimit) {
        return;
      }
      if (
        this.lodash.countBy(this.data, d => d.selected).true > this.selectLimit
      ) {
        this.$toasted.show(
          this.$t("@MaxSelectionCount").replace("@count", this.selectLimit),
          { type: "error" }
        );
        item.selected = false;
      }
    },
    async downloadInvoicePDF(invoiceID){
      let opResult = await this.$api.invoicing.downloadPDF(invoiceID);

      if(opResult.status === "success" && opResult.downloadLinks){
        for(var link of opResult.downloadLinks){
          window.open(link, '_blank');
        }
      }
    }
  }
};
</script>