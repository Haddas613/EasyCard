<template>
  <div v-show="visible" id="transaction-printout">
    <printout-stylesheet></printout-stylesheet>
    <div class="printout">
      <div class="print-entity">
        <div>{{$t('TransactionID')}}</div>
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
        <div>{{$t('SpecialTransactionType')}}</div>
        <div>
          <b v-if="dictionaries.specialTransactionTypeEnum">
            {{dictionaries.specialTransactionTypeEnum[model.specialTransactionType]}}
          </b>
          <b v-else>{{ model.specialTransactionType }}</b>
        </div>
      </div>
      <div class="print-entity">
        <div>{{$t('Origin')}}</div>
        <div>
          <b>{{model.documentOrigin}}</b>
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
      <template v-if="model.numberOfPayments > 1">
        <div class="print-entity">
          <div>{{$t('NumberOfPayments')}}</div>
          <div>
            <b>{{model.numberOfPayments}}</b>
          </div>
        </div>
        <div class="print-entity">
          <div>{{$t('InitialPaymentAmount')}}</div>
          <div>
            <b>{{model.initialPaymentAmount | currency(model.$currency || model.currency)}}</b>
          </div>
        </div>
        <div class="print-entity">
          <div>{{$t('InstallmentPaymentAmount')}}</div>
          <div>
            <b>{{model.installmentPaymentAmount | currency(model.$currency || model.currency)}}</b>
          </div>
        </div>
      </template>
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
      model: this.transaction,
      dictionaries: {},
    };
  },
  async mounted () {
    let $dictionaries = await this.$api.dictionaries.$getTransactionDictionaries();
    this.dictionaries = $dictionaries;
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