<template>
  <v-flex>
    <v-text-field
      v-model="model.cardOwnerName"
      :label="$t('NameOnCard')"
      :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
      background-color="white"
      type="text"
      outlined
    >
      <template v-slot:append>
        <v-icon class="orange--text">mdi-camera-outline</v-icon>
      </template>
    </v-text-field>
    <div class="input-special-group">
      <v-row class="input-special">
      <v-col md="2" cols="3" class="label pr-1">{{$t('CardNo')}}</v-col>
      <v-col cols="7" class="centered">
        <v-row class="input-container">
          <v-col cols="12" class="dense">
            <span class="error--text" v-if="errors['cardNumber']">{{errors['cardNumber']}}</span>
          </v-col>
          <v-col cols="12" class="dense">
            <input
              type="tel"
              class="dense-input"
              ref="cardNumberInp"
              v-cardformat:formatCardNumberOrCardReader
              placeholder="XXXX XXXX XXXX XXXX"
              @keydown.enter="parseCardReader()"
              @input="checkIfCardReader($event)"
            />
          </v-col>
        </v-row>
      </v-col>
      <v-spacer v-if="$vuetify.breakpoint.mdAndUp"></v-spacer>
      <v-col cols="1" class="centered">
        <v-btn icon @click="clearRef('cardNumberInp')">
         <v-icon class="error--text" v-if="errors['cardNumber']">mdi-close</v-icon>
        </v-btn>
      </v-col>
    </v-row>
    <v-row class="input-special">
      <v-col md="2" cols="3" class="label">{{$t('Expiry')}}</v-col>
      <v-col cols="7" class="centered">
        <v-row class="input-container">
          <v-col cols="12" class="dense">
            <span class="error--text" v-if="errors['expiry']">{{errors['expiry']}}</span>
          </v-col>
          <v-col cols="12" class="dense">
            <input
              class="dense-input"
              ref="expiryInp"
              placeholder="MM/YY"
              v-cardformat:formatCardExpiry
            />
          </v-col>
        </v-row>
      </v-col>
      <v-spacer v-if="$vuetify.breakpoint.mdAndUp"></v-spacer>
      <v-col cols="1" class="centered">
        <v-btn icon @click="clearRef('expiryInp')">
          <v-icon class="error--text" v-if="errors['expiry']">mdi-close</v-icon>
        </v-btn>
      </v-col>
    </v-row>
    <v-row class="input-special">
      <v-col md="2" cols="3" class="label">{{$t('CVV')}}</v-col>
      <v-col cols="7" class="centered">
        <v-row class="input-container">
          <v-col cols="12" class="dense">
            <span class="error--text" v-if="errors['cvv']">{{errors['cvv']}}</span>
          </v-col>
          <v-col cols="12" class="dense">
            <input
              class="dense-input"
              type="tel"
              ref="cvvInp"
              placeholder="XXX"
              v-cardformat:formatCardCVC
            />
          </v-col>
        </v-row>
      </v-col>
      <v-spacer v-if="$vuetify.breakpoint.mdAndUp"></v-spacer>
      <v-col cols="1" class="centered">
        <v-btn icon  @click="clearRef('cvvInp')">
          <v-icon class="error--text" v-if="errors['cvv']">mdi-close</v-icon>
        </v-btn>
      </v-col>
    </v-row>
    <v-row class="input-special">
      <v-col md="2" cols="3" class="label">{{$t('NatID')}}</v-col>
      <v-col cols="7" class="centered">
        <v-row class="input-container">
          <v-col cols="12" class="dense">
            <span class="error--text" v-if="errors['nationalId']" @click="clearRef('cvvInp')">{{errors['nationalId']}}</span>
          </v-col>
          <v-col cols="12" class="dense">
            <input
              class="dense-input"
              v-model="model.cardOwnerNationalID"
              placeholder="XXXXXXXXX"
            />
          </v-col>
        </v-row>
      </v-col>
      <v-spacer v-if="$vuetify.breakpoint.mdAndUp"></v-spacer>
      <v-col cols="1" class="centered">
        <v-icon class="error--text" v-if="errors['nationalID']">mdi-close</v-icon>
      </v-col>
    </v-row>
    </div>
  </v-flex>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  props: {
    data: {
      type: Object,
      default: null,
      required: false
    }
  },
  data() {
    return {
      model: { ...this.data },
      vr: ValidationRules,
      errors: {
        cardNumber: false,
        expiry: false,
        cvv: false,
        nationalId: false
      },
      validation: {
        cardOwnerNationalID: [
          ValidationRules.primitives.required,
          ValidationRules.special.israeliNationalId
        ]
      }
    };
  },
  methods: {
    clearRef(refname){
      if(this.$refs[refname]){
        this.$refs[refname].value = null;
      }
    },
    getData() {
      for (var err of Object.keys(this.errors)) {
        this.errors[err] = false;
      }
      if (
        !this.$cardFormat.validateCardNumber(this.$refs.cardNumberInp.value)
      ) {
        this.errors.cardNumber = this.$t("Invalid");
      }

      if (!this.$cardFormat.validateCardCVC(this.$refs.cvvInp.value)) {
        this.errors.cvv = this.$t("Invalid");
      }

      if (!this.$cardFormat.validateCardExpiry(this.$refs.expiryInp.value)) {
        this.errors.expiry = this.$t("Invalid");
      }

      let nationalIdValidation = ValidationRules.primitives.required(
        this.model.cardOwnerNationalID
      );
      if (nationalIdValidation !== true) {
        this.errors.nationalId = this.$t(nationalIdValidation);
      }
      nationalIdValidation = ValidationRules.special.israeliNationalId(
        this.model.cardOwnerNationalID
      );
      if (nationalIdValidation !== true) {
        this.errors.nationalId = this.$t(nationalIdValidation);
      }

      if (!this.lodash.every(this.errors, e => e === false)) return;

      return {
        save: this.model.save,
        cardOwnerName: this.model.cardOwnerName,
        cardNumber: this.$refs.cardNumberInp.value.replace(/\s/g, ""),
        cardExpiration: this.$refs.expiryInp.value.replace(/\s/g, ""),
        cardOwnerNationalID: this.model
          .cardOwnerNationalID,
        cvv: this.$refs.cvvInp.value,
        cardReaderInput: this.model.cardReaderInput
      };
    },
    resetCardReader() {
      this.model.cardReaderInput = null;
      if (this.cardReaderMode) {
        this.cardReaderMode = false;
      }
    },
    checkIfCardReader($event) {
      if (!/^;\d{15,17}=\d{19,21}\?$/.test(this.$refs.cardNumberInp.value)) {
        this.resetCardReader();
        return false;
      }
      return true;
    },
    parseCardReader() {
      if (!this.checkIfCardReader()) return;
      let sep = this.$refs.cardNumberInp.value.split("=");
      this.model.cardReaderInput = this.$refs.cardNumberInp.value;

      console.log(`parseCardReader, split value: ${sep.join("\t")}`);

      //get rid of ';' at the beginning
      this.$refs.cardNumberInp.value = sep[0].substr(
        1,
        sep[0].length
      );
      this.$refs.expiryInp.value = `${sep[1].substr(2, 2)}/${sep[1].substr(
        0,
        2
      )}`;
      this.cardReaderMode = true;
    }
  }
};
</script>