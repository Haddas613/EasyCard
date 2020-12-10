<template>
  <div v-bind:class="{'error--text': token.expired}">
    {{token.cardNumber}}
    <p>
      <small>{{token.cardExpiration}}</small>
    </p>
  </div>
</template>

<script>
export default {
  props: {
    token: {
      type: Object,
      required: true
    }
  }
};
</script>