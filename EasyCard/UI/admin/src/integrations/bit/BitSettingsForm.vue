<template>
  <p>
    {{$t("BitProcessorConnectedMessage")}}
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