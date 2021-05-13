<template>
  <div v-bind:class="{'error--text': token.expired}">
    <span dir="ltr">{{token.cardNumber}}</span>
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