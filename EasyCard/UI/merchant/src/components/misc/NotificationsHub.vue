<template>
  <div>
      <v-btn class="mt-15" color="pink" @click="connectToTransactionsHub()">connect</v-btn>
  </div>
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
        transport: 1
      };

      this.transactionsHub = new signalR.HubConnectionBuilder()
        .withUrl(
            `${this.$cfg.VUE_APP_TRANSACTIONS_API_BASE_ADDRESS}/hubs/transactions`,
            options
        )
        .withAutomaticReconnect()
        //.configureLogging("Trace")
        .build();
        
      this.transactionsHub.on("TransactionStatusChanged", function(payload) {
        console.log(payload)
        // console.log(`TransactionStatusChanged: ${JSON.stringify(payload)}`);
        this.$toasted.show("success", "TransactionStatusChanged");
      });

      this.transactionsHub.start()
        .then(async () => {
            let user = await this.$oidc.getUserProfile();
            this.transactionsHub.invoke('MapConnection', user.sub);
        }).catch(function(err) {
            return console.error(err.toString());
        });
    }
  }
};
</script>