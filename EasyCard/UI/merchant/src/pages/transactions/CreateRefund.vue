<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('CreateRefund')}}</v-card-title>
    <v-divider></v-divider>
    <div>
      <transaction-form :is-refund="true"></transaction-form>
    </div>
  </v-card>
</template>

<script>
import TransactionForm from "../../components/transactions/CreateTransactionForm";

export default {
  components: {
    TransactionForm
  }
};
</script>
