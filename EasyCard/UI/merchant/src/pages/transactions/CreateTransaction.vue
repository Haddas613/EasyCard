<template>
  <v-card class="mx-auto" outlined :loading="loading">
    <v-card-title>{{$t('CreateTransaction')}}</v-card-title>
    <v-divider></v-divider>
    <div>
      <transaction-form></transaction-form>
    </div>
  </v-card>
</template>

<script>
import TransactionForm from "../../components/transactions/CreateTransactionForm";

export default {
  components: {
    TransactionForm
  },
  data() {
    return {
      savedSnackbar: false,
      errorSnackbar: false,
      snackbarErrorsArray: [],
      loading: false
    };
  }
};
</script>