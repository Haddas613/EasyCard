<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-2">
      <v-form class="ec-form" ref="form">
        <v-text-field
          v-model="model.name"
          :label="$t('Name on card')"
          :rules="[vr.primitives.required, vr.primitives.stringLength(3, 50)]"
          background-color="white"
          type="text"
          outlined
        >
          <template v-slot:append>
            <v-icon class="orange--text">mdi-camera-outline</v-icon>
          </template>
        </v-text-field>
        <v-flex class="input-special-group">
          <v-row class="input-special">
            <v-col md="1" cols="3" class="label pr-1">{{$t('CardNo')}}</v-col>
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
                    v-cardformat:formatCardNumber
                    placeholder="XXXX XXXX XXXX XXXX"
                  />
                </v-col>
              </v-row>
            </v-col>
            <v-col cols="1" class="centered">
              <v-icon class="error--text" v-if="errors['cardNumber']">mdi-close</v-icon>
            </v-col>
          </v-row>
          <v-row class="input-special">
            <v-col md="1" cols="3" class="label">{{$t('Expiry')}}</v-col>
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
            <v-col cols="1" class="centered">
              <v-icon class="error--text" v-if="errors['expiry']">mdi-close</v-icon>
            </v-col>
          </v-row>
          <v-row class="input-special">
            <v-col md="1" cols="3" class="label">{{$t('CVV')}}</v-col>
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
            <v-col cols="1" class="centered">
              <v-icon class="error--text" v-if="errors['cvv']">mdi-close</v-icon>
            </v-col>
          </v-row>
          <v-row class="input-special">
            <v-col md="1" cols="3" class="label">{{$t('NatID')}}</v-col>
            <v-col cols="7" class="centered">
              <v-row class="input-container">
                <v-col cols="12" class="dense">
                  <span class="error--text" v-if="errors['nationalID']">{{errors['nationalID']}}</span>
                </v-col>
                <v-col cols="12" class="dense">
                  <input
                    class="dense-input"
                    v-model="model.nationalID"
                    placeholder="XXXXXXXXX"
                    @input="validate('nationalID')"
                  />
                </v-col>
              </v-row>
            </v-col>
            <v-col cols="1" class="centered">
              <v-icon class="error--text" v-if="errors['nationalID']">mdi-close</v-icon>
            </v-col>
          </v-row>
        </v-flex>
        <v-checkbox v-model="model.save" :label="$t('SaveCard')"></v-checkbox>
      </v-form>
    </v-card-text>
    <v-card-actions class="px-0">
      <v-btn color="primary" bottom :x-large="true" fixed block @click="ok()">{{$t('Charge')}}</v-btn>
      <!-- TODO -->
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
  data() {
    return {
      model: {
        save: false
      },
      vr: ValidationRules,
      errors: {
        cardNumber: false,
        expiry: false,
        cvv: false,
        nationalID: false
      },
      validation: {
        // "cardNumber": [ValidationRules.primitives.required],
        // "expiry": [ValidationRules.primitives.required],
        // "cvv": ValidationRules.complex.cvv,
        nationalID: [
          ValidationRules.primitives.required,
          ValidationRules.special.israeliNationalId
        ]
      }
    };
  },
  methods: {
    ok() {
      let form = this.$refs.form.validate();
      for (var err of Object.keys(this.errors)) {
        this.validate(err);
      }

      if (!this.$cardFormat.validateCardNumber(this.$refs.cardNumberInp.value)) {
        this.errors.cardNumber = this.$t("Invalid");
      }

      if (!this.$cardFormat.validateCardCVC(this.$refs.cvvInp.value)) {
        this.errors.cvv = this.$t("Invalid");
      }

      if (!this.$cardFormat.validateCardExpiry(this.$refs.expiryInp.value)) {
        this.errors.expiry = this.$t("Invalid");
      }

      if (!(form && this.lodash.every(this.errors, e => e === false))) 
        return;
        
      this.$emit("ok", {
        save: this.model.save,
        name: this.model.name,
        cardNumber: this.$refs.cardNumberInp.value.replace(/\s/g, ""),
        expiration: this.$refs.expiryInp.value.replace(/\s/g, ""),
        nationalID: this.model.nationalID
      });
    },
    validate(key) {
      if (this.validation[key]) {
        for (var vfn of this.validation[key]) {
          let validationResult = vfn(this.model[key]);
          if (typeof validationResult === "string") {
            this.errors[key] = validationResult;
            return;
          }
        }
      }
      this.errors[key] = false;
    }
  }
};
</script>

<style lang="scss" scoped>
.ec-form {
  .v-input {
    padding-top: 0.5em;
  }
  .dense {
    padding: 0;
    margin: 0;
  }

  .input-special-group {
    .input-special:first-child {
      border-radius: 8px 8px 0 0;
    }
    .input-special:last-child {
      border-radius: 0 0 8px 8px;
      border-bottom: 1px solid #9e9e9e;
    }
  }

  .input-special {
    background-color: white;
    margin: 0 2px;
    border: 1px solid #9e9e9e;
    border-bottom: none;

    .input-container {
      padding-top: 4px;
    }

    .col {
      padding: 0 8px;
      margin: 0;
    }
    .centered {
      display: flex;
      align-self: center;
    }
    .label {
      display: flex;
      align-self: center;
      font-weight: bold;
      font-size: 0.95rem;
    }
    .dense-input {
      padding: 4px 0;
      &:focus {
        outline: none;
      }
    }
  }
}
</style>