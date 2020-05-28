<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('Create Refund')}}</v-card-title>
    <v-divider></v-divider>
    <div>
      <transaction-form v-on:save="onFormSave($event)" :is-refund="true"></transaction-form>
    </div>
  </v-card>
</template>

<script>

import TransactionForm from '../../components/transactions/CreateTransactionForm';

export default {
  components: {
    TransactionForm,
  },
  methods: {
    onFormSave(form) {
      console.log(form);
    }
  },
};
</script>

<style lang="scss" scoped>
</style>