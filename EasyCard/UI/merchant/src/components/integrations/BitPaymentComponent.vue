<template>
  <div class="py-2">
    <div id="bitcom-button-container"></div>
  </div>
</template>


<script>
export default {
  mounted () {
    if(!window.BitPayment){
      var el = document.createElement('script');
      el.src = 'https://public.bankhapoalim.co.il/bitcom/sdk';
      document.head.append(el);
    }
    
    setTimeout(() => {
      window.BitPayment.Buttons({
        onCreate: function(openBitPaymentPage){

          //POST payments/bit/v2/single-payments
          var apiResult = api.createBitTransaction(transaction);

          var transaction = {
            transactionSerialId: apiResult.transactionSerialId,
            paymentInitiationId: apiResult.paymentInitiationId
          };
          openBitPaymentPage(transaction);
        }
      }).render('#bitcom-button-container');
    }, 1000);
  },
  methods: {
    loadBit() {
      
    }
  },
};
</script>