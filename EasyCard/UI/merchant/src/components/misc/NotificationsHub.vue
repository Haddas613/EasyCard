<template>
  <div hidden></div>
</template>

<script>
import * as signalR from "@microsoft/signalr";

export default {
  data() {
    return {
      transactionsHub: null
    };
  },
  async mounted() {
    //await this.connectToTransactionsHub();
  },
  methods: {
    async connectToTransactionsHub() {
      if (this.transactionsHub != null) {
          this.transactionsHub.stop();
      }
      const options = {
        accessTokenFactory: () => {
          return this.$oidc.getAccessToken();
        },
      };
      this.transactionsHub = new signalR.HubConnectionBuilder()
        .withUrl(
          `${this.$cfg.VUE_APP_PROFILE_API_BASE_ADDRESS}/transactionsHub`, options
        )
        .withAutomaticReconnect()
        .build();
        console.log(this.transactionsHub)
      this.transactionsHub.on("TransactionStatusChanged", function(payload) {
        console.log(`TransactionStatusChanged: ${payload}`);
        this.$toasted.show("success", "TransactionStatusChanged");
      });

      this.transactionsHub.start().catch(function(err) {
        return console.error(err.toString());
      });
    }
  }
};
</script>