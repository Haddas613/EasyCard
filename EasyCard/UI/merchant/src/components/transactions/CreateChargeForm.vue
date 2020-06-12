<template>
  <v-card class="ec-card d-flex flex-column">
    <v-card-text class="py-4">
      <v-form class="ec-form">
          <v-text-field
            v-model="model.name"
            :label="$t('Name on card')"
            :rules="[vr.primitives.required, vr.primitives.stringLength(10, 19)]"
            background-color="white"
            type="text"
            outlined
            @keydown.native.space.prevent
          >
           <template v-slot:append>
               <v-icon class="accent--text">mdi-camera</v-icon>
           </template>
          </v-text-field>

          <v-text-field
            v-model="model.cardNumber"
            :label="$t('CardNumber')"
            :rules="[vr.primitives.required, vr.primitives.stringLength(10, 19)]"
            background-color="white"
            placeholder="XXXX XXXX XXXX XXXX"
            type="text"
            outlined
            @keydown.native.space.prevent
          ></v-text-field>
          <v-text-field
            v-model="model.expiry"
            :label="$t('Expiry')"
            :rules="[vr.primitives.required]"
            background-color="white"
            type="text"
            placeholder="MM/YY"
            outlined
            @keydown.native.space.prevent
          ></v-text-field>
          <v-text-field
            v-model="model.cvv"
            :label="$t('CVV')"
            background-color="white"
            type="text"
            placeholder="XXX"
            outlined
            :rules="vr.complex.cvv"
            required
            @keydown.native.space.prevent
          ></v-text-field>

          <v-checkbox class="py-0" v-model="model.save" label="Save card"></v-checkbox>
      </v-form>
    </v-card-text>
    <v-card-actions>
        <v-btn color="primary" width="95vw" :bottom="true" :x-large="true" fixed>Refund</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
import ValidationRules from "../../helpers/validation-rules";

export default {
    data() {
        return {
            model: {},
            vr: ValidationRules
        }
    },
};
</script>

<style lang="scss" scoped>
.ec-card {
  background-color: inherit;
  border: none !important;
  box-shadow: none;
}
.ec-form{
    .v-input{
        padding-top: 1em;
    }
}
</style>