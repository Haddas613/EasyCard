<template>
  <p>
    {{$t("NullAggregatorConnectedMessage")}}
  </p>
</template>

<script>
export default {
  props: {
    data: {
      type: Object,
      required: true
    },
    terminalId: {
      required: true
    },
    apiName: {
      type: String,
      required: true
    }
  },
  data() {
    return {}
  },
};
</script>