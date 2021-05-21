<template>
  <div v-show="visible" id="transaction-printout">
    <printout-stylesheet></printout-stylesheet>
    <div class="printout">
      <div class="print-entity">
        <div>{{$t('ID')}}</div>
        <div>
          <b>{{model.$paymentTransactionID}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('Terminal')}}</div>
        <div>
          <b>{{model.terminalName}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('TransactionType')}}</div>
        <div>
          <b>{{model.transactionType}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('TransactionTime')}}</div>
        <div>
          <b>{{model.$transactionTimestamp | ecdate('LLLL')}}</b>
        </div>
      </div>
      <!-- <div class="print-entity">
        <div>{{$t('VAT')}}</div>
        <div>
          <b>{{(model.vatRate * 100).toFixed(0)}}%</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('VATAmount')}}</div>
        <div>
          <b>{{model.vatTotal | currency(model.$currency)}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('NetAmount')}}</div>
        <div>
          <b>{{model.netTotal | currency(model.$currency)}}</b>
        </div>
      </div> -->
      <div class="print-entity">
        <div>{{$t('Amount')}}</div>
        <div>
          <b>{{model.transactionAmount | currency(model.$currency)}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('CustomerName')}}</div>
        <div>
          <b>{{model.creditCardDetails ? model.creditCardDetails.cardOwnerName : null}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('CardOwnerName')}}</div>
        <div>
          <b>{{model.creditCardDetails.cardOwnerName}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('NationalID')}}</div>
        <div>
          <b>{{model.creditCardDetails.cardOwnerNationalID}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('CardNumber')}}</div>
        <div>
          <b dir="ltr">{{model.creditCardDetails.cardNumber}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('CustomerEmail')}}</div>
        <div>
          <b>{{model.dealDetails.consumerEmail || '-'}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('CustomerPhone')}}</div>
        <div>
          <b>{{model.dealDetails.consumerPhone || '-'}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('DealDescription')}}</div>
        <div>
          <b>{{model.dealDetails.dealDescription}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('ShvaDealID')}}</div>
        <div>
          <b v-if="model.shvaTransactionDetails">{{model.shvaTransactionDetails.shvaDealID}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('ShvaShovarNumber')}}</div>
        <div>
          <b v-if="model.shvaTransactionDetails">{{model.shvaTransactionDetails.shvaShovarNumber}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('ShvaTerminalID')}}</div>
        <div>
          <b v-if="model.shvaTransactionDetails">{{model.shvaTransactionDetails.shvaTerminalID}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('ShvaTransmissionNumber')}}</div>
        <div>
          <b v-if="model.shvaTransactionDetails">{{model.shvaTransactionDetails.shvaTransmissionNumber}}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('Solek')}}</div>
        <div>
          <b v-if="model.shvaTransactionDetails">{{model.shvaTransactionDetails.solek}}</b>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  components: {
    PrintoutStylesheet: () => import("./PrintoutStylesheet")
  },
  props: {
    transaction: {
      type: Object,
      required: true
    },
    visible:{
      type: Boolean,
      required: false,
      default: false
    }
  },
  data() {
    return {
      model: this.transaction
    };
  },
  methods: {
    print() {
      var html = document.querySelector("#transaction-printout");
      var newWindow = window.open("", "PRINT", "height=auto,width=600");
      newWindow.document.write(html.innerHTML);
      newWindow.focus();
      newWindow.print();
      newWindow.close();
    }
  }
};
</script>