<template>
  <!-- <v-card flat class="my-2">
    <template v-if="card">
      <v-card-title
        class="py-3 ecdgray--text subtitle-2 text-uppercase info-block-title"
      >{{$t("BankAccountDetails")}}</v-card-title>
      <v-divider></v-divider>
    </template>
    <v-card-text> -->
      <v-row class="info-container body-1 black--text">
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Bank')}}</p>
          <p>
            <bank-details-string :data="model.bank"></bank-details-string>
          </p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('BankBranch')}}</p>
          <p>{{model.bankBranch || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('BankAccount')}}</p>
          <p>{{model.bankAccount || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('Reference')}}</p>
          <p>{{model.reference || '-'}}</p>
        </v-col>
        <v-col cols="6" md="4" class="info-block">
          <p class="caption ecgray--text text--darken-2">{{$t('DueDate')}}</p>
          <p>
              <span v-if="model.dueDate">{{model.dueDate | ecdate}}</span>
              <span v-else>-</span>
          </p>
        </v-col>
      </v-row>
    <!-- </v-card-text>
  </v-card> -->
</template>

<script>
export default {
  components: {
    BankDetailsString: () => import("../bank/BankDetailsString"),
  },
  props: {
    model: {
      type: Object,
      required: true
    },
    card: {
      type: Boolean,
      default: false
    }
  }
};
</script>