<template>
  <v-container fill-height fluid>
    <v-row align="center" justify="center">
      <v-col class="text-center caption">
        <v-icon class="success--text font-weight-thin" size="170">mdi-check-circle-outline</v-icon>
        <p class="font-weight-bold pt-5">${{amount}}</p>
        <p>Charged to Jan Smith</p>
        <div class="pt-5">
            <p>Invoice sent to</p>
            <p>jan@gmail.com ● 05412345678</p>
        </div>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
    props: {
        //TODO: currency
        amount: {
            type: Number,
            default: 0
        },
        //TODO: implement
        customer: {
            type: Object,
            default: null
        }
    },
};
</script>

<style lang="scss" scoped>
</style>