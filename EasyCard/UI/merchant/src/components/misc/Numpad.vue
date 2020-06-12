<template>
  <div>
    <v-btn color="primary" class="mb-1 text-none" height="48px" @click="ok()" block>{{btntext}} {{amount}}$</v-btn>
    <div>
        <p class="py-8 text-center">Numpad placeholder: work in progress</p>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      amount: new Date().getMilliseconds() / 10 //TODO: only for demo
    };
  },
  props: {
    btntext: {
      type: String,
      default: null
    }
  },
  methods: {
    ok() {
      this.$emit("ok", this.amount);
    }
  }
};
</script>

<style lang="scss" scoped>
</style>