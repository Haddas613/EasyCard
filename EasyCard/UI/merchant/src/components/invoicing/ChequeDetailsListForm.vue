<template>
  <v-row no-gutters>
    <v-col cols="12" class="justify-end">
      <v-btn color="success" @click="addCheque()">
        <v-icon left class="body-1">mdi-plus-circle</v-icon>
        {{ $t("Add") }}
      </v-btn>
    </v-col>
    <v-col cols="12" v-for="(cheque, index) in cheques" :key="index">
      <div class="d-flex justify-end">
        <v-btn @click="deleteCheque(index)" icon color="error"
          ><v-icon>mdi-delete</v-icon></v-btn
        >
      </div>
      <cheque-details-fields
        show-amount
        class="cheque-highligted"
        :ref="`cheque${index}`"
      ></cheque-details-fields>
    </v-col>
  </v-row>
</template>

<script>
export default {
  components: {
    ChequeDetailsFields: () => import("./ChequeDetailsFields.vue"),
  },
  data() {
    return {
      cheques: this.data || [{}],
    };
  },
  methods: {
    addCheque() {
      this.cheques.push({});
    },
    deleteCheque(index) {
      this.cheques.splice(index, 1);
    },
    getData() {
      let result = [];
      for (var i = 0; i < this.cheques.length; i++) {
        if(this.$refs[`cheque${i}`] && this.$refs[`cheque${i}`].length > 0){
          result.push(this.$refs[`cheque${i}`][0].getData());
        }
      }

      return result;
    },
  },
};
</script>

<style lang="scss" scoped>
.cheque-highligted {
  border: 1px solid var(--v-ecgray-base);
  padding: 10px;
  border-radius: 10px;
  margin-top: 1rem;
}
</style>