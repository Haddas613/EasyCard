<template>
  <v-card class="mx-auto" outlined>
    <v-card-title>{{$t('Transactions List')}}</v-card-title>
    <v-divider></v-divider>
    <div>
      <v-data-table :headers="[]" :items="[]" class="elevation-1"></v-data-table>
    </div>
  </v-card>
</template>

<script>
export default {
  name: "TransactionsList"
};
</script>

<style lang="scss" scoped>
</style>