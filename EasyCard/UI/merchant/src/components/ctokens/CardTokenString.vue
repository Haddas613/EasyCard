<template>
  <div v-bind:class="{'error--text': token.expired}">
    <span dir="ltr">
      {{token.cardNumber}}
      <span class="mx-1">{{token.cardExpiration}}</span>
    </span>
    <p class="mb-0 mt-1">
      <small class="black--text">{{token.cardOwnerName}}</small>
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