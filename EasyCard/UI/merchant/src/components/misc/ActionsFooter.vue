<template>
  <v-footer :fixed="true" :padless="true">
    <v-card flat tile width="100%" class="text-center" dark color="grey darken-3">
      <v-card-text class="d-flex justify-space-between pb-0">
        <div v-for="item in items" class="py-0" :key="item.text">
          <v-btn class="white mx-6" icon link :to="item.to">
            <v-icon :class="item.css">{{ item.icon }}</v-icon>
          </v-btn>
          <p class="pt-1">{{item.text}}</p>
        </div>
      </v-card-text>
    </v-card>
  </v-footer>
</template>

<script>
export default {
  data() {
    return {
      items: [
        {
          icon: "mdi-chevron-down-box-outline",
          to: "/admin/transactions/create",
          css: "deep-purple--text",
          text: "Charge"
        },
        {
          icon: "mdi-chevron-left-box-outline",
          css: "orange--text",
          text: "Request"
        },
        {
          icon: "mdi-chevron-up-box-outline",
          to: "/admin/transactions/refund",
          css: "primary--text",
          text: "Refund"
        }
      ]
    };
  }
};
</script>