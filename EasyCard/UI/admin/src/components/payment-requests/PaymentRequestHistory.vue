<template>
  <v-list dense>
    <v-list-item v-for="(h, i) in data" :key="i">
      <v-list-item-icon>
        <v-icon
          v-bind:class="{'circled': !i, 'first': !i}"
          class="icon-padded"
          size="1rem"
          :color="getCodeColor(h.operationCode)"
        >{{i == 0 ? "mdi-circle" : "mdi-circle"}}</v-icon>
      </v-list-item-icon>
      <v-list-item-content>
        <v-list-item-title>{{h.operationMessage}} {{h.operationDate | ecdate("LLLL")}}</v-list-item-title>
      </v-list-item-content>
    </v-list-item>
  </v-list>
</template>
<script>
export default {
  props: {
    data: {
      type: Array,
      required: true
    }
  },
  methods: {
    getCodeColor(code) {
      switch (code) {
        case "Payed":
          return "success";
        case "Rejected":
        case "PaymentFailed":
          return "error";
        case "Viewed":
          return "primary";
        case "Canceled":
          return "accent";
      }
      return "gray";
    }
  }
};
</script>

<style lang="scss" scoped>
.circled {
  border: 1px solid;
  border-radius: 20px;
}
.icon-padded {
  padding: 0px 4px;
  &.first {
    padding: 1px 3px;
  }
}
</style>