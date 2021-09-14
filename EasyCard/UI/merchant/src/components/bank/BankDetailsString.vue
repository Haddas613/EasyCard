<template>
  <span>{{displayModel}}</span>
</template>

<script>
export default {
  props: {
    data: {
      required: true
    }
  },
  data() {
    return {
      displayModel: null,
    };
  },
  async mounted () {
      let banks = await this.$api.dictionaries.getBanks();

      if(this.data){
          let bank = this.lodash.find(banks, e => e.value == this.data);
          this.displayModel = bank ? bank.description : this.data;
      }else{
          this.displayModel = "-";
      }
  }
};
</script>